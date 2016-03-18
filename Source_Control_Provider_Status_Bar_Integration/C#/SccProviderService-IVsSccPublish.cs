using System;
using System.Threading;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.SccProvider
{
    public partial class SccProviderService :
        IVsSccPublish  // This interface has a method which is called when a user initiates a publish workflow which enables a solution to be published to a remote server
    {
        public async Task BeginPublishWorkflowAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IOleCommandTarget oleCommandTarget = ServiceProvider.GlobalProvider.GetService(typeof(SUIHostCommandDispatcher)) as IOleCommandTarget;

            // Execute the add to source control command. In an actual Source Control Provider, query the status before executing the command.
            oleCommandTarget.Exec(GuidList.guidSccProviderCmdSet, CommandId.icmdAddToSourceControl, 0, IntPtr.Zero, IntPtr.Zero);

            cancellationToken.ThrowIfCancellationRequested();

            IVsUIShell uiShell = (IVsUIShell)_sccProvider.GetService(typeof(SVsUIShell));
            if (uiShell != null)
            {
                int result;
                uiShell.ShowMessageBox(dwCompRole: 0,
                                       rclsidComp: Guid.Empty,
                                       pszTitle: Resources.ProviderName,
                                       pszText: Resources.PublishClicked,
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
