using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.AsyncPackageHelpers;

namespace Company.AsyncPackageTest
{
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [Guid(GuidList.guidAsyncPackageTestPkgString)]
    [Microsoft.VisualStudio.AsyncPackageHelpers.PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Microsoft.VisualStudio.AsyncPackageHelpers.ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class AsyncPackageTestPackage : Package, IAsyncLoadablePackageInitialize
    {
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // If async package is not supported, do synchronous initialization
            if (!this.IsAsyncPackageSupported())
            {
                this.BackgroundThreadInitialization();
                IVsShell shellService = this.GetService(typeof(SVsShell)) as IVsShell;
                this.MainThreadInitialization(shellService);
            }
        }

        private void BackgroundThreadInitialization()
        {
            // simulate expensive IO operation
            System.Threading.Thread.Sleep(5000);
        }

        private void MainThreadInitialization(IVsShell shellService)
        {
            // Do operations requiring main thread
        }

        public IVsTask Initialize(IAsyncServiceProvider pServiceProvider, IProfferAsyncService pProfferService, IAsyncProgressCallback pProgressCallback)
        {
            return ThreadHelper.JoinableTaskFactory.RunAsync<object>(async () =>
            {
                this.BackgroundThreadInitialization();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                IVsShell shellService = await pServiceProvider.GetServiceAsync<IVsShell>(typeof(SVsShell));
                this.MainThreadInitialization(shellService);
                return null;
            }).AsVsTask();
        }
    }
}
