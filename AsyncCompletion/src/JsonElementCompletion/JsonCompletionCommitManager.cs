using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace AsyncCompletionSample.JsonElementCompletion
{
    /// <summary>
    /// The simplest implementation of IAsyncCompletionCommitManager that provides Commit Characters and uses default behavior otherwise
    /// </summary>
    internal class JsonCompletionCommitManager : IAsyncCompletionCommitManager
    {
        public JsonCompletionCommitManager()
        {
        }

        ImmutableArray<char> commitChars = new char[] { '"', ',', ':' }.ToImmutableArray();

        public IEnumerable<char> PotentialCommitCharacters => commitChars;

        public bool ShouldCommitCompletion(char typedChar, SnapshotPoint location, CancellationToken token)
        {
            // This method is called only when typedChar is among PotentialCommitCharacters
            // in this simple example, all PotentialCommitCharacters do commit, so we always return true.
            return true;
        }

        public CommitResult TryCommit(ITextView view, ITextBuffer buffer, CompletionItem item, ITrackingSpan applicableToSpan, char typedChar, CancellationToken token)
        {
            return CommitResult.Unhandled; // use default commit mechanism.
        }
    }
}