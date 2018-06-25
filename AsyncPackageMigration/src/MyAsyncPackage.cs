using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace AsyncPackageMigration
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("My Asynchronous Package", "Loads asynchronously", "1.0")]
    [ProvideService(typeof(MyService), IsAsyncQueryable = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid("d71fec50-1ce3-40d5-9e4e-3f5d3ed397b0")]
    public sealed class MyAsyncPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // runs in the background thread and doesn't affect the responsiveness of the UI thread.
            await Task.Delay(5000);

            // Adds a service on the background thread
            AddService(typeof(MyService), CreateMyServiceAsync);            

            // Switches to the UI thread in order to consume some services used in command initialization
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            
            // Query service asynchronously from the UI thread
            var dte = await GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

            // Initializes the command asynchronously now on the UI thread
            await MyCommand.InitializeAsync(this, dte);
        }

        private async Task<object> CreateMyServiceAsync(IAsyncServiceContainer container, CancellationToken cancellationToken, Type serviceType)
        {
            var svc = new MyService();
            await svc.InitializeAsync(this, cancellationToken);
            return svc;
        }
    }
}
