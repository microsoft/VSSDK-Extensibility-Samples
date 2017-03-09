using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;

namespace SpellChecker
{
    class SpellCheckerTagger : ITagger<IErrorTag>, IDisposable
    {
        private readonly SpellChecker _spellChecker;
        private SpellingErrorsSnapshot _spellingErrors;

        internal SpellCheckerTagger(SpellChecker spellChecker)
        {
            _spellChecker = spellChecker;
            _spellingErrors = spellChecker.LastSpellingErrors;

            spellChecker.AddTagger(this);
        }

        internal void UpdateErrors(ITextSnapshot currentSnapshot, SpellingErrorsSnapshot spellingErrors)
        {
            var oldSpellingErrors = _spellingErrors;
            _spellingErrors = spellingErrors;

            var h = this.TagsChanged;
            if (h != null)
            {
                // Raise a single tags changed event over the span that could have been affected by the change in the errors.
                int start = int.MaxValue;
                int end = int.MinValue;

                if ((oldSpellingErrors != null) && (oldSpellingErrors.Errors.Count > 0))
                {
                    start = oldSpellingErrors.Errors[0].Span.Start.TranslateTo(currentSnapshot, PointTrackingMode.Negative);
                    end = oldSpellingErrors.Errors[oldSpellingErrors.Errors.Count - 1].Span.End.TranslateTo(currentSnapshot, PointTrackingMode.Positive);
                }

                if (spellingErrors.Count > 0)
                {
                    start = Math.Min(start, spellingErrors.Errors[0].Span.Start.Position);
                    end = Math.Max(end, spellingErrors.Errors[spellingErrors.Errors.Count - 1].Span.End.Position);
                }

                if (start < end)
                {
                    h(this, new SnapshotSpanEventArgs(new SnapshotSpan(currentSnapshot, Span.FromBounds(start, end))));
                }
            }
        }

        public void Dispose()
        {
            // Called when the tagger is no longer needed (generally when the ITextView is closed).
            _spellChecker.RemoveTagger(this);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (_spellingErrors != null)
            {
                foreach (var error in _spellingErrors.Errors)
                {
                    if (spans.IntersectsWith(error.Span))
                    {
                        yield return new TagSpan<IErrorTag>(error.Span, new ErrorTag(PredefinedErrorTypeNames.Warning));
                    }
                }
            }
        }
    }
}
