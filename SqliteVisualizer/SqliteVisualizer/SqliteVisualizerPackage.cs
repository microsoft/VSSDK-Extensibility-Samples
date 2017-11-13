/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

namespace SqliteVisualizer
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    using Microsoft.VisualStudio.Shell;

    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// Sqlite Visualizer service exposed by the package
    /// </summary>
    [Guid("84755E07-6276-4969-84BB-CB62563AE95E")]
    public interface SSqliteVisualizerService
    {
    }

    /// <summary>
    /// Package providing the visualizer
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideService(typeof(SSqliteVisualizerService), ServiceName = "SqliteVisualizerService", IsAsyncQueryable = true)]
    [Guid(SqliteVisualizerPackage.PackageGuidString)]
    public sealed class SqliteVisualizerPackage : AsyncPackage
    {
        /// <summary>
        /// SqliteVisualizerPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "7178c442-9972-477b-98c1-b495d42dee84";

        /// <inheritdoc />
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            this.AddService(typeof(SSqliteVisualizerService), (sc, ct, st) => Task.FromResult<object>(new SqliteVisualizerService()), true);
        }
    }
}
