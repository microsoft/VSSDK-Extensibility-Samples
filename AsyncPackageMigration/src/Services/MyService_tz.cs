using System;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace AsyncPackageMigration
{
    public class MyService
    {
        private EnvZTA.ZTA _zta;

        public void Initialize(IServiceProvider provider)
        {
           _zta = provider.GetService(typeof(EnvZTA.ZTA)) as EnvZTA.ZTA;
        }

        public async Task InitializeAsync(IAsyncServiceProvider provider, CancellationToken cancellationToken)
        {
            _zta = await provider.GetServiceAsync(typeof(EnvZTA.ZTA)) as EnvZTA.ZTA;
        }
    }
}
