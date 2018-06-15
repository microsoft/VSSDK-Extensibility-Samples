using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text.PatternMatching;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCompletionSample.CompletionItemManager
{
    internal sealed class DefaultCompletionItemManager : IAsyncCompletionItemManager
    {
        readonly IPatternMatcherFactory _patternMatcherFactory;

        internal DefaultCompletionItemManager(IPatternMatcherFactory patternMatcherFactory)
        {
            _patternMatcherFactory = patternMatcherFactory;
        }

        Task<FilteredCompletionModel> IAsyncCompletionItemManager.UpdateCompletionListAsync
            (IAsyncCompletionSession session, AsyncCompletionSessionDataSnapshot data, CancellationToken token)
        {
            var view = session.TextView;
            // Filter by text
            var filterText = session.ApplicableToSpan.GetText(data.Snapshot);
            if (string.IsNullOrWhiteSpace(filterText))
            {
                // There is no text filtering. Just apply user filters, sort alphabetically and return.
                IEnumerable<CompletionItem> listFiltered = data.InitialSortedList;
                if (data.SelectedFilters.Any(n => n.IsSelected))
                {
                    listFiltered = listFiltered.Where(n => ShouldBeInCompletionList(n, data.SelectedFilters));
                }
                var listSorted = listFiltered.OrderBy(n => n.SortText);
                var listHighlighted = listSorted.Select(n => new CompletionItemWithHighlight(n)).ToImmutableArray();
                return Task.FromResult(new FilteredCompletionModel(listHighlighted, 0, data.SelectedFilters));
            }

            // Pattern matcher not only filters, but also provides a way to order the results by their match quality.
            // The relevant CompletionItem is match.Item1, its PatternMatch is match.Item2
            var patternMatcher = _patternMatcherFactory.CreatePatternMatcher(
                filterText,
                new PatternMatcherCreationOptions(System.Globalization.CultureInfo.CurrentCulture, PatternMatcherCreationFlags.IncludeMatchedSpans));

            var matches = data.InitialSortedList
                // Perform pattern matching
                .Select(completionItem => (completionItem, patternMatcher.TryMatch(completionItem.FilterText)))
                // Pick only items that were matched, unless length of filter text is 1
                .Where(n => (filterText.Length == 1 || n.Item2.HasValue));

            // See which filters might be enabled based on the typed code
            var textFilteredFilters = matches.SelectMany(n => n.completionItem.Filters).Distinct();

            // When no items are available for a given filter, it becomes unavailable
            var updatedFilters = ImmutableArray.CreateRange(data.SelectedFilters.Select(n => n.WithAvailability(textFilteredFilters.Contains(n.Filter))));

            // Filter by user-selected filters. The value on availableFiltersWithSelectionState conveys whether the filter is selected.
            var filterFilteredList = matches;
            if (data.SelectedFilters.Any(n => n.IsSelected))
            {
                filterFilteredList = matches.Where(n => ShouldBeInCompletionList(n.completionItem, data.SelectedFilters));
            }

            var bestMatch = filterFilteredList.OrderByDescending(n => n.Item2.HasValue).ThenBy(n => n.Item2).FirstOrDefault();
            var listWithHighlights = filterFilteredList.Select(n => n.Item2.HasValue ? new CompletionItemWithHighlight(n.completionItem, n.Item2.Value.MatchedSpans) : new CompletionItemWithHighlight(n.completionItem)).ToImmutableArray();

            int selectedItemIndex = 0;
            if (data.DisplaySuggestionItem)
            {
                selectedItemIndex = -1;
            }
            else
            {
                for (int i = 0; i < listWithHighlights.Length; i++)
                {
                    if (listWithHighlights[i].CompletionItem == bestMatch.completionItem)
                    {
                        selectedItemIndex = i;
                        break;
                    }
                }
            }

            return Task.FromResult(new FilteredCompletionModel(listWithHighlights, selectedItemIndex, updatedFilters));
        }

        Task<ImmutableArray<CompletionItem>> IAsyncCompletionItemManager.SortCompletionListAsync
            (IAsyncCompletionSession session, AsyncCompletionSessionInitialDataSnapshot data, CancellationToken token)
        {
            return Task.FromResult(data.InitialList.OrderBy(n => n.SortText).ToImmutableArray());
        }

        #region Filtering

        private static bool ShouldBeInCompletionList(
            CompletionItem item,
            ImmutableArray<CompletionFilterWithState> filtersWithState)
        {
            foreach (var filterWithState in filtersWithState.Where(n => n.IsSelected))
            {
                if (item.Filters.Any(n => n == filterWithState.Filter))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
