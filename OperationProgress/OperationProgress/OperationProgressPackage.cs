using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.OperationProgress;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace OperationProgress
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(OperationProgressPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(OperationProgressToolWindow))]
    public sealed class OperationProgressPackage : AsyncPackage
    {
        /// <summary>
        /// OperationProgressPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "54e9361a-1a6f-41be-8a6e-9b1581a493b1";

        public static OperationProgressPackage Instance { get; private set; }

        public IVsOperationProgress VsOperationProgressService { get; private set; }

        public IVsOperationProgressStatusService VsOperationProgressStatusService { get; private set; }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Instance = this;

            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await OperationProgressToolWindowCommand.InitializeAsync(this);

            this.VsOperationProgressService = await this.GetServiceAsync(typeof(SVsOperationProgress)) as IVsOperationProgress;
            // Post Visual Studio 2019 Update 2, should use:
            // this.OperationProgressStatusService = await this.GetServiceAsync(typeof(SVsOperationProgressStatusService)) as IVsOperationProgressStatusService;

            // Version 16.1:
            this.VsOperationProgressStatusService = this.VsOperationProgressService as IVsOperationProgressStatusService;
        }

        #endregion
    }
}
