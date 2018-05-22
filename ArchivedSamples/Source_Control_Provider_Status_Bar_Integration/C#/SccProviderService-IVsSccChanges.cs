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
    public partial class SccProviderService :
        IVsSccChanges // Implementing IVsSccChanges will light up the Pending Changes compartment on the Status Bar
    {
        /// <summary>
        /// Count of the outstanding number of pending changes in the current repository
        /// </summary>
        /// <remarks>
        /// This can be defined by an Source Control provider as to whether it is the number of files, lines, etc. as long as
        /// the information is explained in detail in PendingChangeDetail
        /// </remarks>
        public int PendingChangeCount
        {
            get { return _pendingChangeCount; }
            set
            {
                if (_pendingChangeCount != value)
                {
                    _pendingChangeCount = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PendingChangeCount)));
                }
            }
        }

        private int _pendingChangeCount;

        /// <summary>
        /// Detailed information about the number of outstanding changes in the current repository
        /// </summary>
        public string PendingChangeDetail
        {
            get { return _pendingChangeDetail; }
            set
            {
                if (_pendingChangeDetail != value)
                {
                    _pendingChangeDetail = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PendingChangeDetail)));
                }
            }
        }

        private string _pendingChangeDetail;

        /// <summary>
        /// A label that will be temporarily displayed to indicate busy status
        /// </summary>
        public string PendingChangeLabel
        {
            get { return _pendingChangeLabel; }
            set
            {
                if (_pendingChangeLabel != value)
                {
                    _pendingChangeLabel = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PendingChangeLabel)));
                }
            }
        }

        private string _pendingChangeLabel;

        /// <summary>
        /// Handler called when the pending changes UI is clicked.
        /// A Source Control provider is expected to begin a workflow that will enable the user to 
        /// commit pending changes.
        /// </summary>
        public async Task PendingChangesUIClickedAsync(ISccUIClickedEventArgs args, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Debug.Assert(args != null, "Changes UI coordinates were not received.");

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
            }
        }
    }
}
