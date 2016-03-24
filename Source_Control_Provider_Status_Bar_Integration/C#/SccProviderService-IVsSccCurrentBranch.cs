using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.SccProvider
{
    public partial class SccProviderService :
        IVsSccCurrentBranch   // Interface that has active branch related information
    {
        /// <summary>
        /// The name of the currently active branch
        /// </summary>
        public string BranchName
        {
            get { return _branchName; }
            set
            {
                if (_branchName != value)
                {
                    _branchName = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BranchName)));
                }
            }
        }

        private string _branchName;

        /// <summary>
        /// Details about the currently active branch
        /// </summary>
        public string BranchDetail
        {
            get { return _branchDetail; }
            set
            {
                if (_branchDetail != value)
                {
                    _branchDetail = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BranchDetail)));
                }
            }
        }

        private string _branchDetail;

        /// <summary>
        /// Branch Icon
        /// </summary>
        public ImageMoniker BranchIcon
        {
            get { return _branchIcon; }
            set
            {
                if (!_branchIcon.Equals(value))
                {
                    _branchIcon = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BranchIcon)));
                }
            }
        }

        private ImageMoniker _branchIcon;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Handler called when the branch UI is clicked
        /// </summary>
        /// <remarks>
        /// The UI has an upward arrow visually indicating that a menu would be displayed which when clicked
        /// would lead the user to a workflow that enables the switching of branches
        /// </remarks>
        public async Task BranchUIClickedAsync(ISccUIClickedEventArgs args, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Debug.Assert(args != null, "Branch UI coordinates were not received.");

            IVsUIShell uiShell = (IVsUIShell)_sccProvider.GetService(typeof(SVsUIShell));
            if (uiShell != null)
            {
                POINTS[] p = new POINTS[1];
                p[0] = new POINTS();
                p[0].x = (short)args.ClickedElementPosition.TopRight.X;
                p[0].y = (short)args.ClickedElementPosition.TopRight.Y;

                Guid commandSet = GuidList.guidSccProviderCmdSet;
                uiShell.ShowContextMenu(0, ref commandSet, CommandId.BranchMenu, p, null);
            }
        }
    }
}
