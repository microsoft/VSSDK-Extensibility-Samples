using System;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace AsyncPackageMigration
{
    public class MyService
    {
        private EnvDTE.DTE _dte;

        public void Initialize(IServiceProvider provider)
        {
           _dte = provider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
        }

        public async Task InitializeAsync(IAsyncServiceProvider provider, CancellationToken cancellationToken)
        {
            _dte = await provider.GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
        }
    }
}
