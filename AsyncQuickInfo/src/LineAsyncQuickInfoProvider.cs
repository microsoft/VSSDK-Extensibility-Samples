using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace AsyncQuickInfo
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("Line Async Quick Info Provider")]
    [ContentType("any")]
    [Order]
    internal sealed class LineAsyncQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            // This ensures only one instance per textbuffer is created
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new LineAsyncQuickInfoSource(textBuffer));
        }
    }
}
