using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace AsyncPackageMigration
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("My Synchronous Package", "Loads synchronously", "1.0")]
    [ProvideService(typeof(MyService))]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]
    [Guid("d71fec50-1ce3-40d5-9e4e-3f5d3ed397b0")]
    public class MyPackage : Package
    {
        protected override void Initialize()
        {
            // Long running synchronous method call that blocks the UI thread 
            Thread.Sleep(5000);

            // Adds a service synchronosly on the UI thread
            var callback = new ServiceCreatorCallback(CreateMyService);
            ((IServiceContainer)this).AddService(typeof(MyService), callback);

            // Synchronously requesting a service on the UI thread
            var dte = GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

            // Initializes the command synchronously on the UI thread
            MyCommand.Initialize(this, dte);
        }

        private object CreateMyService(IServiceContainer container, Type serviceType)
        {
            if ( typeof(MyService) == serviceType)
            {
                var svc = new MyService();
                svc.Initialize(this);
                return svc;
            }

            return null;
        }
    }
}
