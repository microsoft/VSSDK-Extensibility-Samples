using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.Text;

namespace SpellChecker
{
    class SpellingError
    {
        public readonly SnapshotSpan Span;
        public readonly IReadOnlyList<string> AlternateSpellings;

        public string _alternatives = null;

        // This is used by SpellingErrorsSnapshot.TranslateTo() to map this error to the corresponding error in the next snapshot.
        public int NextIndex = -1;

        public SpellingError(SnapshotSpan span, IReadOnlyList<string> alternateSpellings)
        {
            this.Span = span;
            this.AlternateSpellings = alternateSpellings;
        }

        public string Alternatives
        {
            get
            {
                if (_alternatives == null)
                {
                    if (this.AlternateSpellings.Count > 0)
                    {
                        StringBuilder b = new StringBuilder();
                        foreach (var alternateSpelling in this.AlternateSpellings)
                        {
                            if (b.Length != 0)
                            {
                                b.Append(", ");
                            }

                            b.Append(alternateSpelling);
                        }

                        _alternatives = b.ToString();
                    }
                }

                return _alternatives;
            }
        }

        public static SpellingError Clone(SpellingError error)
        {
            return new SpellingError(error.Span, error.AlternateSpellings);
        }

        public static SpellingError CloneAndTranslateTo(SpellingError error, ITextSnapshot newSnapshot)
        {
            var newSpan = error.Span.TranslateTo(newSnapshot, SpanTrackingMode.EdgeExclusive);

            // We want to only translate the error if the length of the error span did not change (if it did change, it would imply that
            // there was some text edit inside the error and, therefore, that the error is no longer valid).
            return (newSpan.Length == error.Span.Length)
                   ? new SpellingError(newSpan, error.AlternateSpellings)
                   : null;
        }
    }
}
