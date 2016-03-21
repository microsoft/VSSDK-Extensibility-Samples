using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.SccProvider
{
    /// <summary>
    /// <para>This interface should only be implemented by a Distributed Version Control system such as Git.</para>
    /// <para>This interface has information related to local commits that have not yet been published.</para>                                                                                                          
    /// </summary>
    public partial class SccProviderService :
        IVsSccUnpublishedCommits
    {
        /// <summary>
        /// The number of commits that have not yet been published to a remote server
        /// </summary>
        public int UnpublishedCommitCount
        {
            get { return _unpublishedCommitCount; }
            set
            {
                if (_unpublishedCommitCount != value)
                {
                    _unpublishedCommitCount = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnpublishedCommitCount)));
                }
            }
        }

        private int _unpublishedCommitCount;

        /// <summary>
        /// Details about the number of unpublished commits
        /// </summary>
        public string UnpublishedCommitDetail
        {
            get { return _unpublishedCommitDetail; }
            set
            {
                if (_unpublishedCommitDetail != value)
                {
                    _unpublishedCommitDetail = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnpublishedCommitDetail)));
                }
            }
        }

        private string _unpublishedCommitDetail;

        /// <summary>
        /// A label that will be temporarily displayed to indicate busy status
        /// </summary>
        public string UnpublishedCommitLabel
        {
            get { return _unpublishedCommitLabel; }
            set
            {
                if (_unpublishedCommitLabel != value)
                {
                    _unpublishedCommitLabel = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnpublishedCommitLabel)));
                }
            }
        }

        private string _unpublishedCommitLabel;

        /// <summary>
        /// An event which when raised, let's the VS Shell advertise to the user that the local repository should be backed up
        /// </summary>
        /// <remarks>
        /// This event should only be raised ONCE per repository. We recommend raising this event when the first user initiated commit is executed
        /// by the user
        /// </remarks>
        public event EventHandler AdvertisePublish;

        public async Task UnpublishedCommitsUIClickedAsync(ISccUIClickedEventArgs args, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Debug.Assert(args != null, "Unpublished commits UI coordinates were not received.");

            IVsUIShell uiShell = (IVsUIShell)_sccProvider.GetService(typeof(SVsUIShell));
            if (uiShell != null)
            {
                int result;
                uiShell.ShowMessageBox(dwCompRole: 0,
                                       rclsidComp: Guid.Empty,
                                       pszTitle: Resources.ProviderName,
                                       pszText: string.Format(CultureInfo.CurrentUICulture, Resources.PendingChangesUIClickedMessage, args.ClickedElementPosition.ToString()),
                                       pszHelpFile: string.Empty,
                                       dwHelpContextID: 0,
                                       msgbtn: OLEMSGBUTTON.OLEMSGBUTTON_OK,
                                       msgdefbtn: OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                                       msgicon: OLEMSGICON.OLEMSGICON_INFO,
                                       fSysAlert: 0,        // false = application modal; true would make it system modal
                                       pnResult: out result);

                // Reset the number of published commits when the Unpublished Commits UI is clicked
                UnpublishedCommitCount = 0;
            }
        }

        /// <summary>
        /// Raises the AdvertisePublish event
        /// </summary>
        internal void OnAdvertisePublish()
        {
            AdvertisePublish?.Invoke(this, EventArgs.Empty);
        }
    }
}
