using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ProtocolHandlerSample
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid("4b19d544-cbe1-404c-b751-21698348b48d")]
    [ProvideAppCommandLine(_cliSwitch, typeof(ProtocolPackage), Arguments = "1", DemandLoad = 1)] // More info https://docs.microsoft.com/en-us/visualstudio/extensibility/adding-command-line-switches
    public sealed class ProtocolPackage : AsyncPackage
    {
        private const string _cliSwitch = "MySwitch";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            var cmdline = await GetServiceAsync(typeof(SVsAppCommandLine)) as IVsAppCommandLine;

            ErrorHandler.ThrowOnFailure(cmdline.GetOption(_cliSwitch, out int isPresent, out string optionValue));

            if (isPresent == 1)
            {
                // If opened from a URL, then "optionValue" is the URL string itself
                System.Windows.Forms.MessageBox.Show(optionValue);
            }
        }
    }
}
