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

namespace AsyncCompletionSample.JsonElementCompletion
{
    class JsonCompletionSource : IAsyncCompletionSource
    {
        private ElementCatalog catalog;

        // ImageElements may be shared by CompletionFilters and CompletionItems. The automationName parameter should be localized.
        static ImageElement MetalIcon = new ImageElement(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 2708), "Metal");
        static ImageElement NonMetalIcon = new ImageElement(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 2709), "Non metal");
        static ImageElement MetalloidIcon = new ImageElement(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 2716), "Metalloid");
        static ImageElement UnknownIcon = new ImageElement(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 3533), "Unknown");

        // CompletionFilters are rendered in the UI as buttons
        // The displayText should be localized. Pressing Alt + Access Key acts as clicking on the button.
        static CompletionFilter MetalFilter = new CompletionFilter("Metal", "M", MetalIcon);
        static CompletionFilter NonMetalFilter = new CompletionFilter("Non metal", "N", NonMetalIcon);
        static CompletionFilter UnknownFilter = new CompletionFilter("Unknown", "U", UnknownIcon);

        // CompletionItem takes array of CompletionFilters. In this example, Metalloids use two filters, that is, they are visible in the list if user selects either filter.
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
            // ----- Very simple parsing
            var lineStart = triggerLocation.GetContainingLine().Start;
            var lineEnd = triggerLocation.GetContainingLine().End;
            var spanBeforeCaret = new SnapshotSpan(lineStart, triggerLocation);
            var spanAfterCaret = new SnapshotSpan(triggerLocation, lineEnd);

            var textBeforeCaret = triggerLocation.Snapshot.GetText(spanBeforeCaret);
            var textAfterCaret = triggerLocation.Snapshot.GetText(spanAfterCaret);

            if (token.IsCancellationRequested)
                return CompletionContext.Empty;

            int quoteIndex = -1;
            int colonIndex = -1;
            int numberOfQuotes = 0;
            for (int i = 0; i < textBeforeCaret.Length; i++)
            {
                if (textBeforeCaret[i] == '"')
                {
                    quoteIndex = i;
                    numberOfQuotes++;
                }
                if (textBeforeCaret[i] == ':')
                {
                    colonIndex = i;
                }
            }
            if (numberOfQuotes % 2 == 0)
            {
                // Don't complete when there is an even number of quotes before
                return CompletionContext.Empty;
            }

            PositionInLine position = quoteIndex == -1 ? PositionInLine.Neither : colonIndex == -1 ? PositionInLine.Key : PositionInLine.Value;
            // -----

            switch (position)
            {
                case PositionInLine.Key:
                    return GetContextForKey();
                case PositionInLine.Value:
                    var KeyExtractingRegex = new Regex(@"\s*""(\w+)""\s*:");
                    var key = KeyExtractingRegex.Match(textBeforeCaret);
                    var candidateName = key.Success ? key.Groups.Count > 0 && key.Groups[1].Success ? key.Groups[1].Value : string.Empty : string.Empty;
                     return GetContextForValue(candidateName);
                default:
                    return CompletionContext.Empty;
            }
        }

        private CompletionContext GetContextForValue(string key)
        {
            // Provide a few items based on the key
            ImmutableArray<CompletionItem> itemsBasedOnKey = ImmutableArray<CompletionItem>.Empty;
            if (!string.IsNullOrEmpty(key))
            {
                var matchingElement = catalog.Elements.FirstOrDefault(n => n.Name == key);
                if (matchingElement != null)
                {
                    var itemsBuilder = ImmutableArray.CreateBuilder<CompletionItem>();
                    itemsBuilder.Add(new CompletionItem(matchingElement.Name, this));
                    itemsBuilder.Add(new CompletionItem(matchingElement.Symbol, this));
                    itemsBuilder.Add(new CompletionItem(matchingElement.AtomicNumber.ToString(), this));
                    itemsBuilder.Add(new CompletionItem(matchingElement.AtomicWeight.ToString(), this));
                    itemsBasedOnKey = itemsBuilder.ToImmutable();
                }
            }
            // We would like to allow user to type anything, so we create SuggestionItemOptions
            var suggestionOptions = new SuggestionItemOptions("Value of your choice", $"Please enter value for key {key}");

            return new CompletionContext(itemsBasedOnKey, suggestionOptions);
        }

        private CompletionContext GetContextForKey()
        {
            var context = new CompletionContext(catalog.Elements.Select(n => makeItemFromElement(n)).ToImmutableArray());
            return context;

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
                    case ElementCatalog.Element.Categories.Uncategorized:
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
                    sortText: $"Element {element.AtomicNumber,3}",
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
                return $"{matchingElement.Name} [{matchingElement.AtomicNumber}, {matchingElement.Symbol}] is {getCategoryName(matchingElement.Category)} with atomic weight {matchingElement.AtomicWeight}";
            }
            return null;
        }

        private object getCategoryName(ElementCatalog.Element.Categories category)
        {
            switch(category)
            {
                case ElementCatalog.Element.Categories.Metal: return "a metal";
                case ElementCatalog.Element.Categories.Metalloid: return "a metalloid";
                case ElementCatalog.Element.Categories.NonMetal: return "a non metal";
                default:  return "an uncategorized element";
            }
        }

        public bool TryGetApplicableToSpan(char typeChar, SnapshotPoint triggerLocation, out SnapshotSpan applicableToSpan, CancellationToken token)
        {
            // We trigger completion on demand (typeChar is default) or after user typed quotes
            if (typeChar != default(char) && typeChar != '"')
            {
                applicableToSpan = default(SnapshotSpan);
                return false;
            }
            // Note that triggerLocation.Position is before the quote
            // We want applicableToSpan to start after the quote, hence we add 1

            // ----- Very simple parsing:
            var lineStart = triggerLocation.GetContainingLine().Start;
            var lineEnd = triggerLocation.GetContainingLine().End;
            var spanBeforeCaret = new SnapshotSpan(lineStart, triggerLocation);
            var spanAfterCaret = new SnapshotSpan(triggerLocation, lineEnd);

            var textBeforeCaret = triggerLocation.Snapshot.GetText(spanBeforeCaret);
            var textAfterCaret = triggerLocation.Snapshot.GetText(spanAfterCaret);

            int quoteIndex = -1;
            int colonIndex = -1;
            int numberOfQuotes = 0;
            for (int i = 0; i < textBeforeCaret.Length; i++)
            {
                if (textBeforeCaret[i] == '"')
                {
                    quoteIndex = i;
                    numberOfQuotes++;
                }
                if (textBeforeCaret[i] == ':')
                {
                    colonIndex = i;
                }
            }
            if (numberOfQuotes % 2 == 0)
            {
                // Don't complete when there is an even number of quotes before
                applicableToSpan = default(SnapshotSpan);
                return false;
            }
            PositionInLine position = quoteIndex == -1 ? PositionInLine.Neither : colonIndex == -1 ? PositionInLine.Key : PositionInLine.Value;
            // -----

            switch (position)
            {
                case PositionInLine.Value:
                    {
                        var endIndex = textAfterCaret.IndexOfAny(new char[] { '"', ';', ',', ' ' });
                        int length = 0;
                        if (endIndex == -1)
                        {
                            length = spanAfterCaret.End.Position - triggerLocation.Position - 1;
                        }
                        else
                        {
                            length = endIndex;
                        }

                        if (length < 0)
                            length = 0;

                        applicableToSpan = new SnapshotSpan(triggerLocation.Snapshot, lineStart.Position + quoteIndex + 1, length);
                        return true;
                    }

                case PositionInLine.Key:
                    {
                        var endIndex = textAfterCaret.IndexOfAny(new char[] { '"', ':', ' ', ';', ',' });
                        int length = 0;
                        if (endIndex == -1)
                        {
                            length = spanAfterCaret.End.Position - triggerLocation.Position - 1;
                        }
                        else
                        {
                            length = endIndex;
                        }

                        if (length < 0)
                            length = 0;

                        applicableToSpan = new SnapshotSpan(triggerLocation.Snapshot, lineStart.Position + quoteIndex + 1, length);
                        return true;
                    }

                default:
                    applicableToSpan = default(SnapshotSpan);
                    return false;
            }
        }

        enum PositionInLine
        {
            Key,
            Value,
            Neither
        }
    }
}
