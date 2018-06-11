using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.PatternMatching;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace AsyncCompletionSample.CompletionItemManager
{
    [Export(typeof(IAsyncCompletionItemManagerProvider))]
    [Name(PredefinedCompletionNames.DefaultCompletionItemManager)]
    [ContentType("text")]
    internal sealed class DefaultCompletionItemManagerProvider : IAsyncCompletionItemManagerProvider
    {
        [Import]
        public IPatternMatcherFactory PatternMatcherFactory;

        DefaultCompletionItemManager _instance;

        IAsyncCompletionItemManager IAsyncCompletionItemManagerProvider.GetOrCreate(ITextView textView)
        {
            if (_instance == null)
                _instance = new DefaultCompletionItemManager(PatternMatcherFactory);
            return _instance;
        }
    }
}
