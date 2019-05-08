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
                IVsUIShell shellService = this.GetService(typeof(SVsUIShell)) as IVsUIShell;
                this.MainThreadInitialization(shellService, isAsyncPath: false);
            }
        }


        /// <summary>
        /// Performs the asynchronous initialization for the package in cases where IDE supports AsyncPackage.
        /// 
        /// This method is always called from background thread initially.
        /// </summary>
        /// <param name="asyncServiceProvider">Async service provider instance to query services asynchronously</param>
        /// <param name="pProfferService">Async service proffer instance</param>
        /// <param name="IAsyncProgressCallback">Progress callback instance</param>
        /// <returns></returns>
        public IVsTask Initialize(IAsyncServiceProvider asyncServiceProvider, IProfferAsyncService pProfferService, IAsyncProgressCallback pProgressCallback)
        {
            if (!isAsyncLoadSupported)
            {
                throw new InvalidOperationException("Async Initialize method should not be called when async load is not supported.");
            }

            return ThreadHelper.JoinableTaskFactory.RunAsync<object>(async () =>
            {
                this.BackgroundThreadInitialization();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                IVsUIShell shellService = await asyncServiceProvider.GetServiceAsync<IVsUIShell>(typeof(SVsUIShell));
                this.MainThreadInitialization(shellService, isAsyncPath: true);
                return null;
            }).AsVsTask();
        }
        private void BackgroundThreadInitialization()
        {
            // simulate expensive IO operation
            System.Threading.Thread.Sleep(1000);
        }

        private void MainThreadInitialization(IVsUIShell shellService, bool isAsyncPath)
        {
            // Do operations requiring main thread utilizing passed in services
            shellService.ShowMessageBox(
                   0,
                   Guid.Empty,
                   "BackwardsCompatibleAsyncPackage",
                   "Package initialization is completed using " + (isAsyncPath ? "asynchronous" : "synchronous") + " code path.",
                   string.Empty,
                   0,
                   OLEMSGBUTTON.OLEMSGBUTTON_OK,
                   OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                   OLEMSGICON.OLEMSGICON_INFO,
                   0,        // false
                   out int result);
        }
    }
}
