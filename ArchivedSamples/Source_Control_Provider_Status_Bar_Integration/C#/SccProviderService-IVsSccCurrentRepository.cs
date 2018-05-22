using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.SccProvider
{
    public partial class SccProviderService :
        IVsSccCurrentRepository // Interface that has active repository related information
    {
        /// <summary>
        /// The name of the currently active Repository
        /// </summary>
        public string RepositoryName
        {
            get { return _repositoryName; }
            set
            {
                if (_repositoryName != value)
                {
                    _repositoryName = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RepositoryName)));
                }
            }
        }

        private string _repositoryName;

        /// <summary>
        /// Details about the currently active Repository
        /// </summary>
        public string RepositoryDetail
        {
            get { return _repositoryDetail; }
            set
            {
                if (_repositoryDetail != value)
                {
                    _repositoryDetail = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RepositoryDetail)));
                }
            }
        }

        private string _repositoryDetail;

        /// <summary>
        /// Repository Icon
        /// </summary>
        public ImageMoniker RepositoryIcon
        {
            get { return _repositoryIcon; }
            set
            {
                if (!_repositoryIcon.Equals(value))
                {
                    _repositoryIcon = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RepositoryIcon)));
                }
            }
        }

        private ImageMoniker _repositoryIcon;

        /// <summary>
        /// Handler called when the repository UI is clicked
        /// </summary>
        /// <remarks>
        /// Typically the user would expect to be lead to a workflow that would allow the user to switch repositories
        /// </remarks>
        public async Task RepositoryUIClickedAsync(ISccUIClickedEventArgs args, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Debug.Assert(args != null, "Repository UI coordinates were not received.");

            IVsUIShell uiShell = (IVsUIShell)_sccProvider.GetService(typeof(SVsUIShell));
            if (uiShell != null)
            {
                int result;
                uiShell.ShowMessageBox(dwCompRole: 0,
                                       rclsidComp: Guid.Empty,
                                       pszTitle: Resources.ProviderName,
                                       pszText: string.Format(CultureInfo.CurrentUICulture, Resources.RepositoryUIClickedMessage, args.ClickedElementPosition.ToString()),
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
