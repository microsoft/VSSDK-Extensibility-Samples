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

namespace BackwardsCompatibleAsyncPackage
{
    [Guid(GuidList.guidBackwardsCompatibleAsyncPkgString)]
    [Microsoft.VisualStudio.AsyncPackageHelpers.AsyncPackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Microsoft.VisualStudio.AsyncPackageHelpers.ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class BackwardsCompatibleAsyncPackage : Package, IAsyncLoadablePackageInitialize
    {
        private bool isAsyncLoadSupported;

        /// <summary>
        /// Initialization of the package; this method is always called right after the package is sited on main UI thread of Visual Studio.
        /// 
        /// Both asynchronuos package and synchronous package loading will call this method initially so it is important to skip any initialization
        /// meant for async load phase based on AsyncPackage support in IDE.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            isAsyncLoadSupported = this.IsAsyncPackageSupported();

            // Only perform initialization if async package framework is not supported
            if (!isAsyncLoadSupported)
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
            // Do operations requiring main thread utilizing passed in services
        }

        public IVsTask Initialize(IAsyncServiceProvider pServiceProvider, IProfferAsyncService pProfferService, IAsyncProgressCallback pProgressCallback)
        {
            if (!isAsyncLoadSupported)
            {
                throw new InvalidOperationException("Async Initialize method should not be called when async load is not supported.");
            }

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
