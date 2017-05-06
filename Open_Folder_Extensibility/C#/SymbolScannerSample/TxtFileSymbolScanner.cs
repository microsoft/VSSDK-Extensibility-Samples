using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Indexing;

namespace OpenFolderExtensibility.SymbolScannerSample
{
    /// <summary>
    /// Symbol scanner sample, extracts example symbols from .txt files.
    /// </summary>
    [ExportFileScanner(
        ProviderType,
        "TxtSymbolScanner",
        new String[] { ".txt" },
        new Type[] { typeof(IReadOnlyCollection<SymbolDefinition>) })]
    class TxtFileSymbolScanner : IWorkspaceProviderFactory<IFileScanner>, IFileScanner
    {
        // Unique Guid for TxtSymbolScanner.
        public const string ProviderType = "3CD1C556-6F99-4D5C-A8FD-40C3110E0175";

        public IFileScanner CreateProvider(IWorkspace workspaceContext)
        {
            return this;
        }

        public async Task<T> ScanContentAsync<T>(string filePath, CancellationToken cancellationToken)
            where T : class
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (typeof(T) != typeof(IReadOnlyCollection<SymbolDefinition>))
            {
                throw new NotImplementedException();
            }

            using (StreamReader rdr = new StreamReader(filePath))
            {
                string line;
                int lineNo = 1;
                var results = new List<SymbolDefinition>();
                while ((line = await rdr.ReadLineAsync()) != null)
                {
                    // Extract any line that starts with ` as a symbol and add it to the symbol database for that file.
                    if (line.StartsWith("`"))
                    {
                        results.Add(new SymbolDefinition(line.Substring(1), SymbolKind.None, SymbolAccessibility.None, new TextLocation(lineNo, 1)));
                    }
                    ++lineNo;
                }
                return (T)(IReadOnlyCollection<SymbolDefinition>)results;
            }
        }
    }
}
