/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.Samples.VisualStudio.CodeSweep.Scanner.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.VisualStudio.CodeSweep.Scanner
{
    delegate void MatchFoundCallback(ISearchTerm term, int line, int column, string lineText, string warning);

    class MatchFinder
    {
        /// <summary>
        /// Creates a match finder for the specified set of term tables.
        /// </summary>
        /// <param name="termTables">The set of term tables containing the terms to search for.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>termTables</c> is null.</exception>
        public MatchFinder(IEnumerable<ITermTable> termTables)
        {
            if (termTables == null)
            {
                throw new ArgumentNullException("termTables");
            }

            _termIndex = CreateSortedTermIndex(termTables);
            _exclusionIndex = CreateSortedExclusionIndex(_termIndex);
        }

        /// <summary>
        /// Processes the next character in the sequence.
        /// </summary>
        /// <param name="c">The next character to process.</param>
        /// <param name="callback">The callback for search hits that are found.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>callback</c> is null.</exception>
        /// <remarks>
        /// As a result of this processing, one or more matches may be resolved.  For each match
        /// that is resolved, <c>callback</c> is called.  The call will specify the following
        /// arguments:
        ///     <c>term</c>: non-null
        ///     <c>line</c>: zero-based line number
        ///     <c>column</c>: zero-based column number
        ///     <c>lineText</c>: non-null, non-empty line text
        ///     <c>warning</c>: null in most cases, otherwise the text of a warning if one is relevant
        /// </remarks>
        public void AnalyzeNextCharacter(char c, MatchFoundCallback callback)
        {
            UpdateLineAndColumn(c);
            UpdatePartialMatchesWithNextChar(c);

            // If this is the first character being processed or the previous character was a
            // separator, we will look for new matches and exclusions now.
            if (_lastChar == char.MinValue || IsSeparator(_lastChar))
            {
                FindNewTermsAndExclusionsStartingWith(c);
            }

            DiscardMarkedItems();

            SendAndDiscardConfirmedMatches(callback);

            DiscardMarkedItems();

            _secondToLastChar = _lastChar;
            _lastChar = c;
        }

        /// <summary>
        /// Signals that the end of input has been reached.  Any pending term matches and
        /// exclusion matches will be resolved now, and either confirmed or discarded.
        /// </summary>
        /// <param name="callback">The callback for any matches that are found.</param>
        public void Finish(MatchFoundCallback callback)
        {
            foreach (TermMatch match in PartiallyMatchedTerms)
            {
                match.LineCompleted(_currentLineText);
            }

            SendAndDiscardConfirmedMatches(callback);
        }

        /// <summary>
        /// Resets this match finder to a "clean" state, by removing all cached data and setting
        /// line and column coordinates to zero.
        /// </summary>
        public void Reset()
        {
            _currentColumn = -1;
            _currentLine = 0;
            _lastChar = char.MinValue;
            _secondToLastChar = char.MinValue;
            _partialMatches.Clear();
        }

        #region Private Members

        /// <summary>
        /// Abstract base class for pending term and exclusion matches.
        /// </summary>
        abstract class MatchBase
        {
            abstract public void AddChar(char c);

            public void MarkForDiscard()
            {
                _discardPending = true;
            }

            public bool DiscardPending
            {
                get { return _discardPending; }
            }

            #region Private Members

            bool _discardPending = false;

            #endregion Private Members
        }

        /// <summary>
        /// A pending term match in progress.
        /// </summary>
        class TermMatch : MatchBase
        {
            public TermMatch(MatchFinder finder, MatchableTerm term, int line, int column)
            {
                _finder = finder;
                _term = term;
                _line = line;
                _column = column;
            }

            public MatchableTerm Term
            {
                get { return _term; }
            }

            public int Line
            {
                get { return _line; }
            }

            public int Column
            {
                get { return _column; }
            }

            public string LineText
            {
                get { return _lineText; }
            }

            /// <summary>
            /// Signals that the end of the line on which this term appears has been found.
            /// </summary>
            /// <param name="lineText">The full text of the line.</param>
            public void LineCompleted(string lineText)
            {
                if (_waitingForLineEnd)
                {
                    _lineText = lineText;
                    _waitingForLineEnd = false;
                }

                // Indicate we're no longer waiting for a separator to indicate end-of-word.
                _nextCharMustBeSeparator = false;

                if (IsMatchedAndConfirmed)
                {
                    _finder.RemoveAllMatchesInRangeExceptOne(Line, Column, Line, Column + Term.Term.Text.Length - 1, this);
                }
            }

            public override void AddChar(char c)
            {
                if (!DiscardPending)
                {
                    if (IsMatched)
                    {
                        // The term was already matched.  If we're now watching for the end-of-word
                        // separator, see if this is it.
                        if (_nextCharMustBeSeparator)
                        {
                            if (IsSeparator(c))
                            {
                                _nextCharMustBeSeparator = false;
                            }
                            else
                            {
                                // The character following the term match was not a separator, so
                                // this match is not valid.  Discard it.
                                MarkForDiscard();
                            }
                        }
                    }
                    else
                    {
                        // This match is still in progress.  See if the current character matches
                        // the text we're looking for.
                        if (char.ToLowerInvariant(Term.Term.Text[_matchedChars]) == char.ToLowerInvariant(c))
                        {
                            ++_matchedChars;
                            if (IsMatched)
                            {
                                _nextCharMustBeSeparator = true;
                            }
                        }
                        else
                        {
                            MarkForDiscard();
                        }
                    }

                    // If this term is now matched, discard all other pending matches that overlap
                    // its span.
                    if (IsMatchedAndConfirmed)
                    {
                        _finder.RemoveAllMatchesInRangeExceptOne(Line, Column, Line, Column + Term.Term.Text.Length - 1, this);
                    }
                }
            }

            public bool IsMatchedAndConfirmed
            {
                get { return IsMatched && !HasPartiallyMatchedExclusions && !_waitingForLineEnd && !IsWaitingOnPreviousMatches && !IsWaitingOnSeparator; }
            }

            public bool IsMatched
            {
                get { return !DiscardPending && _matchedChars == Term.Term.Text.Length; }
            }

            #region Private Members

            readonly MatchableTerm _term;
            readonly int _line;
            readonly int _column;
            string _lineText;
            int _matchedChars = 0;
            MatchFinder _finder;
            bool _nextCharMustBeSeparator = false;
            bool _waitingForLineEnd = true;

            private bool HasPartiallyMatchedExclusions
            {
                get
                {
                    foreach (ExclusionMatch match in _finder.PartiallyMatchedExclusions)
                    {
                        if (match.Excludes(this))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            /// <summary>
            /// This match cannot be confirmed until all matches that began earlier have been
            /// discarded, since they take precedence and they may invalidate this match when they
            /// are confirmed.  This property indicates whether any earlier matches are still
            /// active.
            /// </summary>
            private bool IsWaitingOnPreviousMatches
            {
                get
                {
                    foreach (TermMatch match in _finder.PartiallyMatchedTerms)
                    {
                        if (match == this)
                        {
                            // This is the first match.
                            return false;
                        }
                        else
                        {
                            if (!match.DiscardPending)
                            {
                                return true;
                            }
                        }
                    }

                    throw new InvalidOperationException("This match is not in the list; should be impossible.");
                }
            }

            private bool IsWaitingOnSeparator
            {
                get { return _nextCharMustBeSeparator; }
            }

            #endregion Private Members
        }

        /// <summary>
        /// A pending exclusion match in progress.
        /// </summary>
        class ExclusionMatch : MatchBase
        {
            public ExclusionMatch(MatchFinder finder, MatchableExclusion exclusion, int line, int column)
            {
                _finder = finder;
                _exclusion = exclusion;
                _line = line;
                _column = column;
            }

            public override void AddChar(char c)
            {
                if (!IsMatched)
                {
                    if (char.ToLowerInvariant(_exclusion.Text[_matchedChars]) == char.ToLowerInvariant(c))
                    {
                        ++_matchedChars;
                        if (IsMatched)
                        {
                            // When this exclusion is matched, we need to discard all terms it
                            // excludes.
                            foreach (TermMatch match in _finder.PartiallyMatchedTerms)
                            {
                                if (Excludes(match))
                                {
                                    match.MarkForDiscard();
                                }
                            }
                        }
                    }
                    else
                    {
                        MarkForDiscard();
                    }
                }
            }

            public bool Excludes(TermMatch match)
            {
                return match.Term == _exclusion.Term &&
                    RangeContains(_line, _column, _line, _column + _exclusion.Text.Length - 1, match.Line, match.Column) &&
                    RangeContains(_line, _column, _line, _column + _exclusion.Text.Length - 1, match.Line, match.Column + match.Term.Term.Text.Length - 1);
            }

            public bool IsMatched
            {
                get { return _matchedChars == _exclusion.Text.Length; }
            }

            #region Private Members

            readonly MatchFinder _finder;
            readonly MatchableExclusion _exclusion;
            readonly int _line;
            readonly int _column;
            int _matchedChars = 0;

            #endregion Private Members
        }

        /// <summary>
        /// A wrapper for ISearchTerm that stores a few extra properties we need to do matching
        /// correctly.
        /// </summary>
        class MatchableTerm
        {
            readonly ISearchTerm _term;
            readonly int _originalTableIndex;
            readonly int _originalTermIndex;
            bool _hasDuplicates;

            public MatchableTerm(ISearchTerm term, int originalTableIndex, int originalTermIndex)
            {
                _term = term;
                _originalTableIndex = originalTableIndex;
                _originalTermIndex = originalTermIndex;
            }

            public int OriginalTableIndex
            {
                get { return _originalTableIndex; }
            }

            public int OriginalTermIndex
            {
                get { return _originalTermIndex; }
            }

            public ISearchTerm Term
            {
                get { return _term; }
            }

            public bool HasDuplicates
            {
                get { return _hasDuplicates; }
                set { _hasDuplicates = value; }
            }
        }

        /// <summary>
        /// A wrapper for IExclusion that stores a few extra properties we need to do matching
        /// correctly.
        /// </summary>
        class MatchableExclusion
        {
            readonly string _text;
            readonly MatchableTerm _term;

            public MatchableExclusion(string text, MatchableTerm term)
            {
                _text = text;
                _term = term;
            }

            public string Text
            {
                get { return _text; }
            }

            public MatchableTerm Term
            {
                get { return _term; }
            } 
        }

        readonly List<MatchableTerm> _termIndex;
        readonly List<MatchableExclusion> _exclusionIndex;
        readonly List<MatchBase> _partialMatches = new List<MatchBase>();
        int _currentLine = 0;
        int _currentColumn = -1;
        char _lastChar = char.MinValue;
        char _secondToLastChar = char.MinValue;
        string _currentLineText = string.Empty;

        const char LineFeed = (char)10;
        const char CarriageReturn = (char)13;

        private static int CompareStringsWithLongestFirst(string x, string y)
        {
            int comparison = string.Compare(x, 0, y, 0, Math.Min(x.Length, y.Length), StringComparison.OrdinalIgnoreCase);
            if (comparison == 0)
            {
                if (x.Length > y.Length)
                {
                    return -1;
                }
                else if (x.Length < y.Length)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return comparison;
            }
        }

        private static List<MatchableExclusion> CreateSortedExclusionIndex(List<MatchableTerm> sortedTermIndex)
        {
            List<MatchableExclusion> index = new List<MatchableExclusion>();

            foreach (MatchableTerm term in sortedTermIndex)
            {
                foreach (IExclusion exclusion in term.Term.Exclusions)
                {
                    index.Add(new MatchableExclusion(exclusion.Text, term));
                }
            }

            index.Sort((x, y) => CompareStringsWithLongestFirst(x.Text, y.Text));

            return index;
        }

        private static List<MatchableTerm> CreateSortedTermIndex(IEnumerable<ITermTable> termTables)
        {
            List<MatchableTerm> index = new List<MatchableTerm>();
            int tableIndex = 0;

            foreach (ITermTable table in termTables)
            {
                if (table.Terms != null)
                {
                    int termIndex = 0;
                    foreach (ISearchTerm term in table.Terms)
                    {
                        if (term.Text.Length > 0)
                        {
                            index.Add(new MatchableTerm(term, tableIndex, termIndex));
                        }
                        ++termIndex;
                    }
                }
                ++tableIndex;
            }

            index.Sort(CompareMatchableTerms);

            for (int i = 0; i < index.Count; ++i)
            {
                if ((i > 0 && string.Compare(index[i].Term.Text, index[i - 1].Term.Text, StringComparison.OrdinalIgnoreCase) == 0) ||
                    (i < index.Count - 1 && string.Compare(index[i].Term.Text, index[i + 1].Term.Text, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    index[i].HasDuplicates = true;
                }
            }

            return index;
        }

        static int CompareMatchableTerms(MatchableTerm x, MatchableTerm y)
        {
            int result = CompareStringsWithLongestFirst(x.Term.Text, y.Term.Text);
            if (result == 0)
            {
                // For duplicate terms, we want to put them in the order they were
                // specified.
                if (x.OriginalTableIndex != y.OriginalTableIndex)
                {
                    result = x.OriginalTableIndex - y.OriginalTableIndex;
                }
                else
                {
                    result = x.OriginalTermIndex - y.OriginalTermIndex;
                }
            }
            return result;
        }

        private void SendAndDiscardConfirmedMatches(MatchFoundCallback callback)
        {
            foreach (TermMatch match in ConfirmedMatches)
            {
                if (callback != null)
                {
                    string warning = null;

                    // Matches with duplicates will have a warning attached.
                    if (match.Term.HasDuplicates)
                    {
                        StringBuilder tableList = new StringBuilder();
                        bool first = true;
                        foreach (MatchableTerm term in GetDuplicatesOf(match.Term))
                        {
                            if (!first)
                            {
                                tableList.Append("; ");
                            }
                            tableList.Append(term.Term.Table.SourceFile);
                            first = false;
                        }

                        warning = string.Format(CultureInfo.CurrentUICulture, Resources.DuplicateTermWarning, match.Term.Term.Text, tableList.ToString());
                    }
                    callback(match.Term.Term, match.Line, match.Column, match.LineText, warning);
                }
                match.MarkForDiscard();
            }
        }

        private IEnumerable<MatchableTerm> GetDuplicatesOf(MatchableTerm sourceTerm)
        {
            return _termIndex.Where(
                possibleDup => string.Compare(sourceTerm.Term.Text, possibleDup.Term.Text, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private void DiscardMarkedItems()
        {
            for (int i = 0; i < _partialMatches.Count; ++i)
            {
                if (_partialMatches[i].DiscardPending)
                {
                    _partialMatches.RemoveAt(i);
                    --i;
                }
            }
        }

        private static bool IsSeparator(char c)
        {
            return (char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsSymbol(c)) && c != '_';
        }

        private void UpdateLineAndColumn(char c)
        {
            if (StartsNewLine(c))
            {
                ++_currentLine;
                _currentColumn = 0;

                foreach (TermMatch match in PartiallyMatchedTerms)
                {
                    match.LineCompleted(_currentLineText);
                }
                _currentLineText = "" + c;
            }
            else
            {
                if (c != CarriageReturn && c != LineFeed)
                {
                    ++_currentColumn;
                    _currentLineText += c;
                }
            }
        }

        private bool StartsNewLine(char c)
        {
            if (_secondToLastChar == CarriageReturn && _lastChar == LineFeed)
            {
                // <cr><lf>
                return true;
            }
            else if (_lastChar == CarriageReturn && c != LineFeed)
            {
                // <cr> alone
                return true;
            }
            else if (_lastChar == LineFeed)
            {
                // <lf> alone
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UpdatePartialMatchesWithNextChar(char c)
        {
            foreach (MatchBase match in PartiallyMatchedTermsAndExclusions)
            {
                match.AddChar(c);
            }
        }

        private IEnumerable<TermMatch> PartiallyMatchedTerms
        {
            get { return _partialMatches.OfType<TermMatch>(); }
        }

        private IEnumerable<ExclusionMatch> PartiallyMatchedExclusions
        {
            get { return _partialMatches.OfType<ExclusionMatch>(); }
        }

        private IEnumerable<MatchBase> PartiallyMatchedTermsAndExclusions
        {
            get { return _partialMatches; }
        }

        private void FindNewTermsAndExclusionsStartingWith(char c)
        {
            foreach (MatchableTerm term in TermsStartingWith(c))
            {
                TermMatch match = new TermMatch(this, term, _currentLine, _currentColumn);
                _partialMatches.Add(match);
                match.AddChar(c);
            }

            foreach (MatchableExclusion exclusion in ExclusionsStartingWith(c))
            {
                ExclusionMatch match = new ExclusionMatch(this, exclusion, _currentLine, _currentColumn);
                _partialMatches.Add(match);
                match.AddChar(c);
            }
        }

        private IEnumerable<MatchableTerm> TermsStartingWith(char c)
        {
            return BinarySearch(_termIndex,
                term => char.ToLowerInvariant(term.Term.Text[0]) - char.ToLowerInvariant(c));
        }

        private IEnumerable<MatchableExclusion> ExclusionsStartingWith(char c)
        {
            return BinarySearch(_exclusionIndex,
                exclusion => char.ToLowerInvariant(exclusion.Text[0]) - char.ToLowerInvariant(c));
        }

        private delegate int BinarySearchDelegate<T>(T t);

        private static IEnumerable<T> BinarySearch<T>(IList<T> index, BinarySearchDelegate<T> tester)
        {
            if (index.Count > 0)
            {
                return BinarySearch(index, 0, index.Count - 1, tester);
            }
            else
            {
                return new List<T>();
            }
        }

        /// <summary>
        /// Finds all items in the specified range of the specified sorted list for which the
        /// given tester returns 0.
        /// </summary>
        private static IEnumerable<T> BinarySearch<T>(IList<T> index, int first, int last, BinarySearchDelegate<T> tester)
        {
            if (index == null)
            {
                throw new ArgumentNullException("index");
            }
            if (tester == null)
            {
                throw new ArgumentNullException("tester");
            }
            if (first < 0)
            {
                throw new ArgumentOutOfRangeException("first");
            }
            if (last >= index.Count || last < first)
            {
                throw new ArgumentOutOfRangeException("last");
            }

            int middle = (first + last) / 2;
            int middleTest = tester(index[middle]);

            if (middleTest > 0)
            {
                if (middle > first)
                {
                    return BinarySearch(index, first, middle - 1, tester);
                }
                else
                {
                    return new List<T>();
                }
            }
            else if (middleTest < 0)
            {
                if (last > middle)
                {
                    return BinarySearch(index, middle + 1, last, tester);
                }
                else
                {
                    return new List<T>();
                }
            }
            else
            {
                return ExtendMatchingRange(index, middle, tester);
            }
        }

        private static IEnumerable<T> ExtendMatchingRange<T>(IList<T> index, int knownMatchIndex, BinarySearchDelegate<T> tester)
        {
            int dummyFirst = 0;
            int dummyLast = 0;
            return ExtendMatchingRange(index, knownMatchIndex, tester, out dummyFirst, out dummyLast);
        }

        private static IEnumerable<T> ExtendMatchingRange<T>(IList<T> index, int knownMatchIndex, BinarySearchDelegate<T> tester, out int first, out int last)
        {
            if (index == null)
            {
                throw new ArgumentNullException("index");
            }
            if (tester == null)
            {
                throw new ArgumentNullException("tester");
            }
            if (knownMatchIndex < 0 || knownMatchIndex >= index.Count)
            {
                throw new ArgumentOutOfRangeException("knownMatchIndex");
            }

            first = knownMatchIndex;
            last = knownMatchIndex;

            while (first > 0 && tester(index[first - 1]) == 0)
            {
                --first;
            }

            while (last + 1 < index.Count && tester(index[last + 1]) == 0)
            {
                ++last;
            }

            List<T> result = new List<T>();

            for (int i = first; i <= last; ++i)
            {
                result.Add(index[i]);
            }

            return result;
        }

        private IEnumerable<TermMatch> ConfirmedMatches
        {
            get
            {
                foreach (TermMatch match in PartiallyMatchedTerms)
                {
                    if (match.IsMatchedAndConfirmed)
                    {
                        yield return match;
                    }
                    else
                    {
                        if (!match.DiscardPending)
                        {
                            // No match can be returned as a hit until everything before it has been
                            // confirmed or discarded, so we won't look past the first unconfirmed
                            // match.
                            yield break;
                        }
                    }
                }
            }
        }

        void RemoveAllMatchesInRangeExceptOne(int lineStart, int columnStart, int lineEnd, int columnEnd, TermMatch matchToSave)
        {
            foreach (TermMatch match in PartiallyMatchedTerms)
            {
                if (match != matchToSave && RangesOverlap(lineStart, columnStart, lineEnd, columnEnd, match.Line, match.Column, match.Line, match.Column + match.Term.Term.Text.Length - 1))
                {
                    match.MarkForDiscard();
                }
            }
        }

        private static bool RangesOverlap(int firstLineStart, int firstColumnStart, int firstLineEnd, int firstColumnEnd, int secondLineStart, int secondColumnStart, int secondLineEnd, int secondColumnEnd)
        {
            if (firstLineEnd < secondLineStart)
            {
                return false;
            }

            if (secondLineEnd < firstLineStart)
            {
                return false;
            }

            // At this point, we know they at least touch the same line (either firstLineEnd >=
            // secondLineStart or secondLineEnd >= firstLineStart), but they may not overlap.

            if (firstLineEnd == secondLineStart)
            {
                return firstColumnEnd >= secondColumnStart;
            }

            if (secondLineEnd == firstLineStart)
            {
                return secondColumnEnd >= firstColumnStart;
            }

            return true;
        }

        private static bool RangeContains(int lineStart, int columnStart, int lineEnd, int columnEnd, int line, int column)
        {
            if (line < lineStart || line > lineEnd)
            {
                return false;
            }

            if (line == lineStart && column < columnStart)
            {
                return false;
            }

            if (line == lineEnd && column > columnEnd)
            {
                return false;
            }

            return true;
        }

        #endregion Private Members
    }
}
