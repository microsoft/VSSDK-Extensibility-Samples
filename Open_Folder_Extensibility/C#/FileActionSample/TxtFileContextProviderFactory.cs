using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Workspace;
using Task = System.Threading.Tasks.Task;
using OpenFolderExtensibility.VSPackage;

namespace OpenFolderExtensibility.FileActionSample
{

    /// <summary>
    /// File context provider for TXT files.
    /// </summary>
    [ExportFileContextProvider(ProviderType, PackageIds.TxtFileContextType)]
    class TxtFileContextProviderFactory : IWorkspaceProviderFactory<IFileContextProvider>
    {
        // Unique Guid for TxtFileContextProvider.
        private const string ProviderType = "5091157D-3DC8-40E5-989F-8E0D31F76CA2";

        /// <inheritdoc/>
        public IFileContextProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new TxtFileContextProvider(workspaceContext);
        }

        private class TxtFileContextProvider : IFileContextProvider
        {
            private IWorkspace workspaceContext;

            internal TxtFileContextProvider(IWorkspace workspaceContext)
            {
                this.workspaceContext = workspaceContext;
            }

            /// <inheritdoc />
            public async Task<IReadOnlyCollection<FileContext>> GetContextsForFileAsync(string filePath, CancellationToken cancellationToken)
            {
                var fileContexts = new List<FileContext>();

                if (filePath.EndsWith(".txt"))
                {
                    fileContexts.Add(new FileContext(
                        new Guid(ProviderType),
                        new Guid(PackageIds.TxtFileContextType),
                        filePath + "\n",
                        Array.Empty<string>()));
                }

                return await Task.FromResult(fileContexts.ToArray());
            }
        }
    }
}
