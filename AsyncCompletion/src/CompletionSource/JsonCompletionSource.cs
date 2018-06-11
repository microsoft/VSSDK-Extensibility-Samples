using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCompletionSample.CompletionSource
{
    class JsonCompletionSource : IAsyncCompletionSource
    {
        private ElementCatalog catalog;

        static ImageElement MetalIcon = new ImageElement(new ImageId(), "Metal");
        static ImageElement NonMetalIcon = new ImageElement(new ImageId(), "Non metal");
        static ImageElement MetalloidIcon = new ImageElement(new ImageId(), "Metalloid");
        static ImageElement UnknownIcon = new ImageElement(new ImageId(), "Unknown");
        static CompletionFilter MetalFilter = new CompletionFilter("Metal", "M", MetalIcon);
        static CompletionFilter NonMetalFilter = new CompletionFilter("Non metal", "N", NonMetalIcon);
        static CompletionFilter UnknownFilter = new CompletionFilter("Unknown", "U", UnknownIcon);
        static ImmutableArray<CompletionFilter> MetalFilters = ImmutableArray.Create(MetalFilter);
        static ImmutableArray<CompletionFilter> NonMetalFilters = ImmutableArray.Create(NonMetalFilter);
        static ImmutableArray<CompletionFilter> MetalloidFilters = ImmutableArray.Create(MetalFilter, NonMetalFilter);
        static ImmutableArray<CompletionFilter> UnknownFilters = ImmutableArray.Create(UnknownFilter);

        public JsonCompletionSource(ElementCatalog catalog)
        {
            this.catalog = catalog;
        }

        public async Task<CompletionContext> GetCompletionContextAsync(InitialTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var lineStart = triggerLocation.GetContainingLine().Start;
            var lineEnd = triggerLocation.GetContainingLine().Start;
            var spanBeforeCaret = new SnapshotSpan(lineStart, triggerLocation);
            var spanAfterCaret = new SnapshotSpan(triggerLocation, lineEnd);

            var textBeforeCaret = triggerLocation.Snapshot.GetText(spanBeforeCaret);
            var textAfterCaret = triggerLocation.Snapshot.GetText(spanAfterCaret);

            if (token.IsCancellationRequested)
                return CompletionContext.Empty;

            switch (GetPosition(textBeforeCaret))
            {
                case PositionInLine.Key:
                    return GetContextForKey();
                case PositionInLine.Value:
                    var KeyExtractingRegex = new Regex(@"\s*""(\w +)""\s*:");
                    var key = KeyExtractingRegex.Match(textBeforeCaret);
                    return GetContextForValue(key.Value);
                default:
                    return CompletionContext.Empty;
            }
        }

        private CompletionContext GetContextForValue(string key)
        {
            // Provide a few items based on the key
            ImmutableArray<CompletionItem> itemsBasedOnKey;
            var matchingElement = catalog.Elements.FirstOrDefault(n => n.Name == key);
            if (matchingElement == null)
            {
                itemsBasedOnKey = ImmutableArray<CompletionItem>.Empty;
            }
            else
            {
                var itemsBuilder = ImmutableArray.CreateBuilder<CompletionItem>();
                itemsBasedOnKey.Add(new CompletionItem(matchingElement.Name, this));
                itemsBasedOnKey.Add(new CompletionItem(matchingElement.Symbol, this));
                itemsBasedOnKey.Add(new CompletionItem(matchingElement.AtomicNumber.ToString(), this));
                itemsBasedOnKey.Add(new CompletionItem(matchingElement.AtomicWeight.ToString(), this));
                itemsBasedOnKey = itemsBuilder.ToImmutable();
            }
            // We would like to allow user to type anything, so we create SuggestionItemOptions
            var suggestionOptions = new SuggestionItemOptions("Value of your choice", $"Please enter value for key {key}");

            return new CompletionContext(itemsBasedOnKey, suggestionOptions);
        }

        private CompletionContext GetContextForKey()
        {
            return new CompletionContext(catalog.Elements.Select(n => makeItemFromElement(n)).ToImmutableArray());

            CompletionItem makeItemFromElement(ElementCatalog.Element element)
            {
                ImageElement icon = null;
                ImmutableArray<CompletionFilter> filters;

                switch (element.Category)
                {
                    case ElementCatalog.Element.Categories.Metal:
                        icon = MetalIcon;
                        filters = MetalFilters;
                        break;
                    case ElementCatalog.Element.Categories.Metalloid:
                        icon = MetalloidIcon;
                        filters = MetalloidFilters;
                        break;
                    case ElementCatalog.Element.Categories.NonMetal:
                        icon = NonMetalIcon;
                        filters = NonMetalFilters;
                        break;
                    case ElementCatalog.Element.Categories.Unknown:
                        icon = UnknownIcon;
                        filters = UnknownFilters;
                        break;
                }
                var item = new CompletionItem(
                    displayText: element.Name,
                    source: this,
                    icon: icon,
                    filters: filters,
                    suffix: string.Empty,
                    insertText: element.Name,
                    sortText: $"Element {element.AtomicNumber}",
                    filterText: $"{element.Name} {element.Symbol}",
                    attributeIcons: ImmutableArray<ImageElement>.Empty);

                // Each completion item has a property bag
                item.Properties.AddProperty(nameof(ElementCatalog.Element), element);

                return item;
            }
        }

        public async Task<object> GetDescriptionAsync(CompletionItem item, CancellationToken token)
        {
            if (item.Properties.TryGetProperty<ElementCatalog.Element>(nameof(ElementCatalog.Element), out var matchingElement))
            {
                return $"{matchingElement.Name} [{matchingElement.AtomicNumber}, {matchingElement.Symbol}] with atomic weight {matchingElement.AtomicWeight}";
            }
            return null;
        }

        private PositionInLine GetPosition(string textBeforeCaret)
        {
            var quoteIndex = textBeforeCaret.LastIndexOf('"');
            if (quoteIndex == -1)
            {
                return PositionInLine.Neither;
            }

            var colonIndex = textBeforeCaret.LastIndexOf(':');
            return colonIndex == -1 ? PositionInLine.Value : PositionInLine.Key;
        }

        enum PositionInLine
        {
            Key,
            Value,
            Neither
        }

        public bool TryGetApplicableToSpan(char typeChar, SnapshotPoint triggerLocation, out SnapshotSpan applicableToSpan, CancellationToken token)
        {
            // We trigger completion on demand (typeChar is default) or after user typed quotes
            if (typeChar != default && typeChar != '"')
            {
                applicableToSpan = default;
                return false;
            }

            var lineStart = triggerLocation.GetContainingLine().Start;
            var lineEnd = triggerLocation.GetContainingLine().Start;
            var spanBeforeCaret = new SnapshotSpan(lineStart, triggerLocation);
            var spanAfterCaret = new SnapshotSpan(triggerLocation, lineEnd);

            var textBeforeCaret = triggerLocation.Snapshot.GetText(spanBeforeCaret);
            var textAfterCaret = triggerLocation.Snapshot.GetText(spanAfterCaret);
            var quoteIndex = textBeforeCaret.LastIndexOf('"');

            switch (GetPosition(textBeforeCaret))
            {
                case PositionInLine.Value:
                    {
                        var endIndex = textAfterCaret.IndexOfAny(new char[] { '"', ';', ',', ' ' });
                        if (endIndex == -1)
                            endIndex = spanAfterCaret.End;

                        applicableToSpan = new SnapshotSpan(triggerLocation.Snapshot, quoteIndex, endIndex);
                        return true;
                    }

                case PositionInLine.Key:
                    {
                        var endIndex = textAfterCaret.IndexOfAny(new char[] { '"', ':', ' ', ';', ',' });
                        if (endIndex == -1)
                            endIndex = spanAfterCaret.End;

                        applicableToSpan = new SnapshotSpan(triggerLocation.Snapshot, quoteIndex, endIndex);
                        return true;
                    }

                default:
                    applicableToSpan = default;
                    return false;
            }
        }
    }
}
