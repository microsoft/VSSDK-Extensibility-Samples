namespace OperationProgress
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft;
    using Microsoft.VisualStudio.OperationProgress;
    using Microsoft.VisualStudio.Threading;

    /// <summary>
    /// Interaction logic for OperationProgressToolWindowControl.
    /// </summary>
    public partial class OperationProgressToolWindowControl : UserControl, IDisposable
    {
        const string StageId = CommonOperationProgressStageIds.Intellisense;

        /// <summary>
        /// Visual Studio Operation Progress Service, that enables registering work with Operation Progress.
        /// </summary>
        private readonly IVsOperationProgress vsOperationProgressService;

        /// <summary>
        /// Visual Studio Operation Progress Status Service, that offers information about operations in progress.
        /// </summary>
        private readonly IVsOperationProgressStatusService vsOperationProgressStatusService;

        /// <summary>
        /// Joinable Task Factory.
        /// </summary>
        private readonly JoinableTaskFactory joinableTaskFactory;

        /// <summary>
        /// Provides access to the IntelliSense stage
        /// </summary>
        IOperationProgressStageAccess intelliSenseStageAccess;

        /// <summary>
        /// Task Completion Source that represents the work submitted to the IntelliSense stage.
        /// </summary>
        private TaskCompletionSource<bool> taskCompletionSource;

        /// <summary>
        /// Provides status information about the IntelliSense stage.
        /// </summary>
        private IVsOperationProgressStageStatus intelliSenseStageStatus;

        /// <summary>
        /// Tracks if the object is disposed to detect reduntant calls.
        /// </summary>
        private bool isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationProgressToolWindowControl"/> class.
        /// </summary>
        public OperationProgressToolWindowControl()
        {
            this.vsOperationProgressService = OperationProgressPackage.Instance.VsOperationProgressService;
            this.vsOperationProgressStatusService = OperationProgressPackage.Instance.VsOperationProgressStatusService;
            this.joinableTaskFactory = OperationProgressPackage.Instance.JoinableTaskFactory;

            this.InitializeComponent();

            this.intelliSenseStageStatus = vsOperationProgressStatusService.GetStageStatus(CommonOperationProgressStageIds.Intellisense);

            this.intelliSenseStageStatus.InProgressChanged += IntelliSenseStatus_InProgressChanged;

            // Update the status textblock with the current value
            this.UpdateintelliSenseStatusTextBlock(intelliSenseStageStatus.Status);
        }

        /// <summary>
        /// Registers Test work with the Operation Progress service.
        /// </summary>
        private void IntelliSenseCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (this.intelliSenseStageAccess != null)
            {
                return;
            }

            // Get access to the stage
            this.intelliSenseStageAccess = vsOperationProgressService.AccessStage(StageId, "test", 1);

            // Create a task that tracks the work and register it with the operation progress
            this.taskCompletionSource = new TaskCompletionSource<bool>();

            // Create a JoinableTask that represents the work.
            // We use JoinableTask to allow JoinableTaskFactory to track the entire chain between operation progress producers and awaiters in order to avoid deadlocks.
            JoinableTask joinableTask = this.joinableTaskFactory.RunAsync(() => taskCompletionSource.Task);

            intelliSenseStageAccess.RegisterTask(new OperationProgressTask(joinableTask, "Test", () => Task.FromResult("Test")));
        }

        /// <summary>
        /// Completes work registered in <see cref="IntelliSenseCheckBox_Checked(object, RoutedEventArgs)"/>.
        /// </summary>
        private void IntelliSenseCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Complete the task
            this.taskCompletionSource.SetResult(true);
            this.taskCompletionSource = null;

            // Close the access to the stage
            this.intelliSenseStageAccess.Dispose();
            this.intelliSenseStageAccess = null;
        }

        /// <summary>
        /// Receives notifications when the status of the IntelliSense stage has changed and updates the UI. Note that events are received on background threads, so it needs to switch to the UI thread in order to update the UI.
        /// </summary>
        private void IntelliSenseStatus_InProgressChanged(object sender, OperationProgressStatusChangedEventArgs e)
        {
            // Operation Progress service sends notifications on background threads.
            // In order to update the UI, we need to switch to the main thread first.
            this.joinableTaskFactory.RunAsync(
                async () =>
                {
                    await this.joinableTaskFactory.SwitchToMainThreadAsync();

                    this.UpdateintelliSenseStatusTextBlock(e.Status);
                });
        }

        /// <summary>
        /// Helper method that displays the status in the corresponding TextBlock.
        /// </summary>
        /// <param name="operationProgressStageStatus">The status to display.</param>
        private void UpdateintelliSenseStatusTextBlock(OperationProgressStageStatus operationProgressStageStatus)
        {
            Verify.Operation(this.joinableTaskFactory.Context.IsOnMainThread, "Should only be called on main thread.");

            string status = string.Format(
                "{0} ({1})",
                operationProgressStageStatus.IsInProgress ? "In Progress" : "Complete",
                operationProgressStageStatus.Version);
            this.intelliSenseStatusTextBlock.Text = status;
        }

        /// <summary>
        /// Simulates awaiting for completion of operation in progress by disabling the button and updating its text.
        /// When the operation completes, it switches to the UI thread and reverts the button to the initial state.
        /// </summary>
        private void WaitForIntelliSenseStage_Click(object sender, RoutedEventArgs e)
        {
            this.waitForIntelliSenseStageButton.IsEnabled = false;
            object originalContent = this.waitForIntelliSenseStageButton.Content;
            this.waitForIntelliSenseStageButton.Content = "Waiting...";

            this.joinableTaskFactory.RunAsync(
                async () =>
                {
                    await this.intelliSenseStageStatus.WaitForCompletionAsync();

                    // Switch to the UI thread to restore the state of the Button
                    await this.joinableTaskFactory.SwitchToMainThreadAsync();
                    this.waitForIntelliSenseStageButton.Content = originalContent;
                    this.waitForIntelliSenseStageButton.IsEnabled = true;
                });

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.intelliSenseStageStatus.InProgressChanged -= IntelliSenseStatus_InProgressChanged;
                }

                this.isDisposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }
    }
}