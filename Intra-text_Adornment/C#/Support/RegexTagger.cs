//***************************************************************************
//
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace IntraTextAdornmentSample
{
    /// <summary>
    /// Helper base class for writing simple taggers based on regular expressions.
    /// </summary>
    /// <remarks>
    /// Regular expressions are expected to be single-line.
    /// </remarks>
    /// <typeparam name="T">The type of tags that will be produced by this tagger.</typeparam>
    internal abstract class RegexTagger<T> : ITagger<T> where T : ITag
    {
        private readonly IEnumerable<Regex> matchExpressions;

        public RegexTagger(ITextBuffer buffer, IEnumerable<Regex> matchExpressions)
        {
            if (matchExpressions.Any(re => (re.Options & RegexOptions.Multiline) == RegexOptions.Multiline))
                throw new ArgumentException("Multiline regular expressions are not supported.");

            this.matchExpressions = matchExpressions;

            buffer.Changed += (sender, args) => HandleBufferChanged(args);

        }

        #region ITagger implementation

        public virtual IEnumerable<ITagSpan<T>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            // Here we grab whole lines so that matches that only partially fall inside the spans argument are detected.
            // Note that the spans argument can contain spans that are sub-spans of lines or intersect multiple lines.
            foreach (var line in GetIntersectingLines(spans))
            {
                string text = line.GetText();

                foreach (var regex in matchExpressions)
                {
                    foreach (var match in regex.Matches(text).Cast<Match>())
                    {
                        T tag = TryCreateTagForMatch(match);
                        if (tag != null)
                        {
                            SnapshotSpan span = new SnapshotSpan(line.Start + match.Index, match.Length);
                            yield return new TagSpan<T>(span, tag);
                        }
                    }
                }
            }
        }
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        #endregion

        IEnumerable<ITextSnapshotLine> GetIntersectingLines(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;
            int lastVisitedLineNumber = -1;
            ITextSnapshot snapshot = spans[0].Snapshot;
            foreach (var span in spans)
            {
                int firstLine = snapshot.GetLineNumberFromPosition(span.Start);
                int lastLine = snapshot.GetLineNumberFromPosition(span.End);

                for (int i = Math.Max(lastVisitedLineNumber, firstLine); i <= lastLine; i++)
                {
                    yield return snapshot.GetLineFromLineNumber(i);
                }

                lastVisitedLineNumber = lastLine;
            }
        }

        /// <summary>
        /// Overridden in the derived implementation to provide a tag for each regular expression match.
        /// If the return value is <c>null</c>, this match will be skipped.
        /// </summary>
        /// <param name="match">The match to create a tag for.</param>
        /// <returns>The tag to return from <see cref="GetTags"/>, if non-<c>null</c>.</returns>
        protected abstract T TryCreateTagForMatch(Match match);

        /// <summary>
        /// Handle buffer changes. The default implementation expands changes to full lines and sends out
        /// a <see cref="TagsChanged"/> event for these lines.
        /// </summary>
        /// <param name="args">The buffer change arguments.</param>
        protected virtual void HandleBufferChanged(TextContentChangedEventArgs args)
        {
            if (args.Changes.Count == 0)
                return;

            var temp = TagsChanged;
            if (temp == null)
                return;

            // Combine all changes into a single span so that
            // the ITagger<>.TagsChanged event can be raised just once for a compound edit
            // with many parts.

            ITextSnapshot snapshot = args.After;

            int start = args.Changes[0].NewPosition;
            int end = args.Changes[args.Changes.Count - 1].NewEnd;

            SnapshotSpan totalAffectedSpan = new SnapshotSpan(
                snapshot.GetLineFromPosition(start).Start,
                snapshot.GetLineFromPosition(end).End);

            temp(this, new SnapshotSpanEventArgs(totalAffectedSpan));
        }
    }
}
