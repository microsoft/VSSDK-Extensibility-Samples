using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Operations;
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
    class SampleCompletionSource : IAsyncCompletionSource
    {
        private ElementCatalog Catalog { get; }
        private ITextStructureNavigatorSelectorService StructureNavigatorSelector { get; }

        // ImageElements may be shared by CompletionFilters and CompletionItems. The automationName parameter should be localized.
        static ImageElement MetalIcon = new ImageElement(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 2708), "Metal");
        static ImageElement NonMetalIcon = new ImageElement(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 2709), "Non metal");
        static ImageElement MetalloidIcon = new ImageElement(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 2716), "Metalloid");
        static ImageElement UnknownIcon = new ImageElement(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 3533), "Unknown");

        // CompletionFilters are rendered in the UI as buttons
        // The displayText should be localized. Alt + Access Key toggles the filter button.
        static CompletionFilter MetalFilter = new CompletionFilter("Metal", "M", MetalIcon);
        static CompletionFilter NonMetalFilter = new CompletionFilter("Non metal", "N", NonMetalIcon);
        static CompletionFilter UnknownFilter = new CompletionFilter("Unknown", "U", UnknownIcon);

        // CompletionItem takes array of CompletionFilters.
        // In this example, items assigned "MetalloidFilters" are visible in the list if user selects either MetalFilter or NonMetalFilter.
        static ImmutableArray<CompletionFilter> MetalFilters = ImmutableArray.Create(MetalFilter);
        static ImmutableArray<CompletionFilter> NonMetalFilters = ImmutableArray.Create(NonMetalFilter);
        static ImmutableArray<CompletionFilter> MetalloidFilters = ImmutableArray.Create(MetalFilter, NonMetalFilter);
        static ImmutableArray<CompletionFilter> UnknownFilters = ImmutableArray.Create(UnknownFilter);

        public SampleCompletionSource(ElementCatalog catalog, ITextStructureNavigatorSelectorService structureNavigatorSelector)
        {
            Catalog = catalog;
            StructureNavigatorSelector = structureNavigatorSelector;
        }

        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            // We don't trigger completion when user typed
            if (char.IsNumber(trigger.Character)         // a number
                || char.IsPunctuation(trigger.Character) // punctuation
                || trigger.Character == '\n'             // new line
                || trigger.Reason == CompletionTriggerReason.Backspace
                || trigger.Reason == CompletionTriggerReason.Deletion)
            {
                return CompletionStartData.DoesNotParticipateInCompletion;
            }

            // We participate in completion and provide the "applicable to span".
            // This span is used:
            // 1. To search (filter) the list of all completion items
            // 2. To highlight (bold) the matching part of the completion items
            // 3. In standard cases, it is replaced by content of completion item upon commit.

            // If you want to extend a language which already has completion, don't provide a span, e.g.
            // return CompletionStartData.ParticipatesInCompletionIfAny

            // If you provide a language, but don't have any items available at this location,
            // consider providing a span for extenders who can't parse the codem e.g.
            // return CompletionStartData(CompletionParticipation.DoesNotProvideItems, spanForOtherExtensions);

            var tokenSpan = FindTokenSpanAtPosition(triggerLocation);
            return new CompletionStartData(CompletionParticipation.ProvidesItems, tokenSpan);
        }

        private SnapshotSpan FindTokenSpanAtPosition(SnapshotPoint triggerLocation)
        {
            // This method is not really related to completion,
            // we mostly work with the default implementation of ITextStructureNavigator 
            // You will likely use the parser of your language
            ITextStructureNavigator navigator = StructureNavigatorSelector.GetTextStructureNavigator(triggerLocation.Snapshot.TextBuffer);
            TextExtent extent = navigator.GetExtentOfWord(triggerLocation);
            if (triggerLocation.Position > 0 && (!extent.IsSignificant || !extent.Span.GetText().Any(c => char.IsLetterOrDigit(c))))
            {
                // Improves span detection over the default ITextStructureNavigation result
                extent = navigator.GetExtentOfWord(triggerLocation - 1);
            }

            var tokenSpan = triggerLocation.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);

            var snapshot = triggerLocation.Snapshot;
            var tokenText = tokenSpan.GetText(snapshot);
            if (string.IsNullOrWhiteSpace(tokenText))
            {
                // The token at this location is empty. Return an empty span, which will grow as user types.
                return new SnapshotSpan(triggerLocation, 0);
            }

            // Trim quotes and new line characters.
            int startOffset = 0;
            int endOffset = 0;

            if (tokenText.Length > 0)
            {
                if (tokenText.StartsWith("\""))
                    startOffset = 1;
            }
            if (tokenText.Length - startOffset > 0)
            {
                if (tokenText.EndsWith("\"\r\n"))
                    endOffset = 3;
                else if (tokenText.EndsWith("\r\n"))
                    endOffset = 2;
                else if (tokenText.EndsWith("\"\n"))
                    endOffset = 2;
                else if (tokenText.EndsWith("\n"))
                    endOffset = 1;
                else if (tokenText.EndsWith("\""))
                    endOffset = 1;
            }

            return new SnapshotSpan(tokenSpan.GetStartPoint(snapshot) + startOffset, tokenSpan.GetEndPoint(snapshot) - endOffset);
        }

        public async Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            // See whether we are in the key or value portion of the pair
            var lineStart = triggerLocation.GetContainingLine().Start;
            var spanBeforeCaret = new SnapshotSpan(lineStart, triggerLocation);
            var textBeforeCaret = triggerLocation.Snapshot.GetText(spanBeforeCaret);
            var colonIndex = textBeforeCaret.IndexOf(':');
            var colonExistsBeforeCaret = colonIndex != -1;

            // User is likely in the key portion of the pair
            if (!colonExistsBeforeCaret)
                return GetContextForKey();

            // User is likely in the value portion of the pair. Try to provide extra items based on the key.
            var KeyExtractingRegex = new Regex(@"\W*(\w+)\W*:");
            var key = KeyExtractingRegex.Match(textBeforeCaret);
            var candidateName = key.Success ? key.Groups.Count > 0 && key.Groups[1].Success ? key.Groups[1].Value : string.Empty : string.Empty;
            return GetContextForValue(candidateName);
        }

        /// <summary>
        /// Returns completion items applicable to the value portion of the key-value pair
        /// </summary>
        private CompletionContext GetContextForValue(string key)
        {
            // Provide a few items based on the key
            ImmutableArray<CompletionItem> itemsBasedOnKey = ImmutableArray<CompletionItem>.Empty;
            if (!string.IsNullOrEmpty(key))
            {
                var matchingElement = Catalog.Elements.FirstOrDefault(n => n.Name == key);
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

        /// <summary>
        /// Returns completion items applicable to the key portion of the key-value pair
        /// </summary>
        private CompletionContext GetContextForKey()
        {
            var context = new CompletionContext(Catalog.Elements.Select(n => MakeItemFromElement(n)).ToImmutableArray());
            return context;
        }

        /// <summary>
        /// Builds a <see cref="CompletionItem"/> based on <see cref="ElementCatalog.Element"/>
        /// </summary>
        private CompletionItem MakeItemFromElement(ElementCatalog.Element element)
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
                suffix: element.Symbol,
                insertText: element.Name,
                sortText: $"Element {element.AtomicNumber,3}",
                filterText: $"{element.Name} {element.Symbol}",
                attributeIcons: ImmutableArray<ImageElement>.Empty);

            // Each completion item we build has a reference to the element in the property bag.
            // We use this information when we construct the tooltip.
            item.Properties.AddProperty(nameof(ElementCatalog.Element), element);

            return item;
        }

        /// <summary>
        /// Provides detailed element information in the tooltip
        /// </summary>
        public async Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
        {
            if (item.Properties.TryGetProperty<ElementCatalog.Element>(nameof(ElementCatalog.Element), out var matchingElement))
            {
                return $"{matchingElement.Name} [{matchingElement.AtomicNumber}, {matchingElement.Symbol}] is {GetCategoryName(matchingElement.Category)} with atomic weight {matchingElement.AtomicWeight}";
            }
            return null;
        }

        private string GetCategoryName(ElementCatalog.Element.Categories category)
        {
            switch(category)
            {
                case ElementCatalog.Element.Categories.Metal: return "a metal";
                case ElementCatalog.Element.Categories.Metalloid: return "a metalloid";
                case ElementCatalog.Element.Categories.NonMetal: return "a non metal";
                default:  return "an uncategorized element";
            }
        }
    }
}
