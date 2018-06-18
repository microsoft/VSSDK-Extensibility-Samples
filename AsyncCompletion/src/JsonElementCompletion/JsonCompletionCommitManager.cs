using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace AsyncCompletionSample.JsonElementCompletion
{
    internal class JsonCompletionCommitManager : IAsyncCompletionCommitManager
    {
        public JsonCompletionCommitManager()
        {
        }

        ImmutableArray<char> commitChars = new char[] { '"', ',', ':' }.ToImmutableArray();

        public IEnumerable<char> PotentialCommitCharacters => commitChars;

        public bool ShouldCommitCompletion(char typeChar, SnapshotPoint location, CancellationToken token)
        {
            return true;
        }

        public CommitResult TryCommit(ITextView view, ITextBuffer buffer, CompletionItem item, ITrackingSpan applicableToSpan, char typedChar, CancellationToken token)
        {
            return CommitResult.Unhandled; // use default commit
        }
    }
}