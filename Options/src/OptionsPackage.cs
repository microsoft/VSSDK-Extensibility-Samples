using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using OptionsSample.Options;
using Task = System.Threading.Tasks.Task;

namespace OptionsSample
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideOptionPage(typeof(DialogPageProvider.General), "My Options", "General", 0, 0, true)]
    [ProvideOptionPage(typeof(DialogPageProvider.Other), "My Options", "Other", 0, 0, true)]
    [Guid("8bb519a5-4864-43b0-8684-e2f2f723100c")]
    public sealed class OptionsPackage : AsyncPackage
    {
        protected override Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            return base.InitializeAsync(cancellationToken, progress);
        }
    }
}
