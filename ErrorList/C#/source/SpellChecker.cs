using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SpellChecker
{
    ///<summary>
    /// Finds the spelling errors in comments for a specific buffer.
    ///</summary>
    /// <remarks><para>The lifespan of this object is tied to the lifespan of the taggers on the view. On creation of the first tagger, the SpellChecker starts doing
    /// work to find errors in the file. On the disposal of the last tagger, it shuts down.</para>
    /// </remarks>
    public class SpellChecker
    {
        private readonly SpellCheckerProvider _provider;
        private readonly ITextBuffer _buffer;
        private readonly Dispatcher _uiThreadDispatcher;
        private readonly TextBox _box = new TextBox();      // Used to do spell checking.

        private IClassifier _classifier;

        private ITextSnapshot _currentSnapshot;
        private NormalizedSnapshotSpanCollection _dirtySpans;

        private bool _isUpdating = false;
        private bool _isDisposed = false;

        private readonly List<SpellCheckerTagger> _activeTaggers = new List<SpellCheckerTagger>();

        internal readonly string FilePath;
        internal readonly SpellingErrorsFactory Factory;

        internal SpellChecker(SpellCheckerProvider provider, ITextView textView, ITextBuffer buffer)
        {
            _provider = provider;
            _buffer = buffer;
            _currentSnapshot = buffer.CurrentSnapshot;

            // Get the name of the underlying document buffer
            ITextDocument document;
            if (provider.TextDocumentFactoryService.TryGetTextDocument(textView.TextDataModel.DocumentBuffer, out document))
            {
                this.FilePath = document.FilePath;

                // TODO we should listen for the file changing its name (ITextDocument.FileActionOccurred)
            }

            // turn on spell checking for the box we're using to do spell checking.
            _box.SpellCheck.IsEnabled = true;

            // We're assuming we're created on the UI thread so capture the dispatcher so we can do all of our updates on the UI thread.
            _uiThreadDispatcher = Dispatcher.CurrentDispatcher;

            this.Factory = new SpellingErrorsFactory(this, new SpellingErrorsSnapshot(this.FilePath, 0));
        }

        internal void AddTagger(SpellCheckerTagger tagger)
        {
            _activeTaggers.Add(tagger);

            if (_activeTaggers.Count == 1)
            {
                // First tagger created ... start doing stuff.
                _classifier = _provider.ClassifierAggregatorService.GetClassifier(_buffer);

                _buffer.ChangedLowPriority += this.OnBufferChange;

                _dirtySpans = new NormalizedSnapshotSpanCollection(new SnapshotSpan(_currentSnapshot, 0, _currentSnapshot.Length));

                _provider.AddSpellChecker(this);

                this.KickUpdate();
            }
        }

        internal void RemoveTagger(SpellCheckerTagger tagger)
        {
            _activeTaggers.Remove(tagger);

            if (_activeTaggers.Count == 0)
            {
                // Last tagger was disposed of. This is means there are no longer any open views on the buffer so we can safely shut down
                // spell checking for that buffer.
                _buffer.ChangedLowPriority -= this.OnBufferChange;

                _provider.RemoveSpellChecker(this);

                _isDisposed = true;

                _buffer.Properties.RemoveProperty(typeof(SpellChecker));

                IDisposable classifierDispose = _classifier as IDisposable;
                if (classifierDispose != null)
                    classifierDispose.Dispose();

                _classifier = null;
            }
        }

        private void OnBufferChange(object sender, TextContentChangedEventArgs e)
        {
            _currentSnapshot = e.After;

            // Translate all of the old dirty spans to the new snapshot.
            NormalizedSnapshotSpanCollection newDirtySpans = _dirtySpans.CloneAndTrackTo(e.After, SpanTrackingMode.EdgeInclusive);

            // Dirty all the spans that changed.
            foreach (var change in e.Changes)
            {
                newDirtySpans = NormalizedSnapshotSpanCollection.Union(newDirtySpans, new NormalizedSnapshotSpanCollection(e.After, change.NewSpan));
            }

            // Translate all the spelling errors to the new snapshot (and remove anything that is a dirty region since we will need to check that again).
            var oldSpenningErrors = this.Factory.CurrentSnapshot;
            var newSpellingErrors = new SpellingErrorsSnapshot(this.FilePath, oldSpenningErrors.VersionNumber + 1);

            // Copy all of the old errors to the new errors unless the error was affected by the text change
            foreach (var error in oldSpenningErrors.Errors)
            {
                Debug.Assert(error.NextIndex == -1);

                var newError = SpellingError.CloneAndTranslateTo(error, e.After);

                if (newError != null)
                {
                    Debug.Assert(newError.Span.Length == error.Span.Length);

                    error.NextIndex = newSpellingErrors.Errors.Count;
                    newSpellingErrors.Errors.Add(newError);
                }
            }

            this.UpdateSpellingErrors(newSpellingErrors);

            _dirtySpans = newDirtySpans;

            // Start processing the dirty spans (which no-ops if we're already doing it).
            if (_dirtySpans.Count != 0)
            {
                this.KickUpdate();
            }
        }

        private void KickUpdate()
        {
            // We're assuming we will only be called from the UI thread so there should be no issues with race conditions.
            if (!_isUpdating)
            {
                _isUpdating = true;
                _uiThreadDispatcher.BeginInvoke(new Action(() => this.DoUpdate()), DispatcherPriority.Background);
            }
        }

        private void DoUpdate()
        {
            // It would be good to do all of this work on a background thread but we can't:
            //      _classifier.GetClassificationSpans() should only be called on the UI thread because some classifiers assume they are called from the UI thread.
            //      Raising the TagsChanged event from the taggers needs to happen on the UI thread (because some consumers might assume it is being raised on the UI thread).
            // 
            // Updating the snapshot for the factory and calling the sink can happen on any thread but those operations are so fast that there is no point.
            if ((!_isDisposed) && (_dirtySpans.Count > 0))
            {
                var line = _dirtySpans[0].Start.GetContainingLine();

                if (line.Length > 0)
                {
                    var oldSpellingErrors = this.Factory.CurrentSnapshot;
                    var newSpellingErrors = new SpellingErrorsSnapshot(this.FilePath, oldSpellingErrors.VersionNumber + 1);

                    // Go through the existing errors. If they are on the line we are currently parsing then
                    // copy them to oldLineErrors, otherwise they go to the new errors.
                    var oldLineErrors = new List<SpellingError>();
                    foreach (var error in oldSpellingErrors.Errors)
                    {
                        Debug.Assert(error.NextIndex == -1);

                        if (line.Extent.Contains(error.Span))
                        {
                            error.NextIndex = -1;
                            oldLineErrors.Add(error);                           // Do not clone old error here ... we'll do that later there is no change.
                        }
                        else
                        {
                            error.NextIndex = newSpellingErrors.Errors.Count;
                            newSpellingErrors.Errors.Add(SpellingError.Clone(error));   // We must clone the old error here.
                        }
                    }

                    int expectedErrorCount = newSpellingErrors.Errors.Count + oldLineErrors.Count;
                    bool anyNewErrors = false;

                    var classifications = _classifier.GetClassificationSpans(line.Extent);
                    foreach (var classification in classifications)
                    {
                        if (classification.ClassificationType.IsOfType(PredefinedClassificationTypeNames.Comment) || classification.ClassificationType.IsOfType("xml doc comment - text"))
                        {
                            string text = classification.Span.GetText();

                            _box.Text = text;

                            int index = 0;
                            while (index < text.Length)
                            {
                                int errorStart = _box.GetNextSpellingErrorCharacterIndex(index, System.Windows.Documents.LogicalDirection.Forward);
                                if (errorStart < 0)
                                {
                                    break;
                                }

                                int errorLength = _box.GetSpellingErrorLength(errorStart);
                                if (errorLength > 1)    // Ignore any single character misspelling.
                                {
                                    var newSpan = new SnapshotSpan(classification.Span.Start + errorStart, errorLength);
                                    if (SpellChecker.IsPossibleSpellingError(newSpan))
                                    {
                                        var oldError = oldLineErrors.Find((e) => e.Span == newSpan);

                                        if (oldError != null)
                                        {
                                            // There was a spelling error at the same span as the old one so we should be able to just reuse it.
                                            oldError.NextIndex = newSpellingErrors.Errors.Count;
                                            newSpellingErrors.Errors.Add(SpellingError.Clone(oldError));    // Don't clone the old error yet
                                        }
                                        else
                                        {
                                            // Let WPF decide whether or not there are any suggested spellings.
                                            var wpfSpellngError = _box.GetSpellingError(errorStart);

                                            newSpellingErrors.Errors.Add(new SpellingError(newSpan, new List<string>(wpfSpellngError.Suggestions)));
                                            anyNewErrors = true;
                                        }
                                    }

                                    index = errorStart + errorLength;
                                }
                                else
                                {
                                    // How can you have a spelling error with a length of 0? Handle it gracefully in any case.
                                    index = errorStart + 1;
                                }
                            }
                        }
                    }

                    // This does a deep comparison so we will only do the update if the a different set of errors was discovered compared to what we had previously.
                    // If there were any new errors or if we didn't see all the expected errors then there is a change and we need to update the spelling errors.
                    if (anyNewErrors || (newSpellingErrors.Errors.Count != expectedErrorCount))
                    {
                        this.UpdateSpellingErrors(newSpellingErrors);
                    }
                    else
                    {
                        // There were no changes so we don't need to update our snapshot.
                        // We have, however, dirtied the old errors by setting their NextIndex property on the assumption that we would be updating the errors.
                        // Revert all those changes.
                        foreach (var error in oldSpellingErrors.Errors)
                        {
                            error.NextIndex = -1;
                        }
                    }
                }

                // NormalizedSnapshotSpanCollection.Difference doesn't quite do what we need here. If I have {[5,5), [10,20)} and subtract {[5, 15)} and do a ...Difference, I
                // end up with {[5,5), [15,20)} (the zero length span at the beginning isn't getting removed). A zero-length span at the end wouldn't be removed but, in this case,
                // that is the desired behavior (the zero length span at the end could be a change at the beginning of the next line, which we'd want to keep).
                var newDirtySpans = new List<Span>(_dirtySpans.Count + 1);
                var extent = line.ExtentIncludingLineBreak;

                for (int i = 0; (i < _dirtySpans.Count); ++i)
                {
                    Span s = _dirtySpans[i];
                    if ((s.End < extent.Start) || (s.Start >= extent.End))          // Intentionally asymmetric
                    {
                        newDirtySpans.Add(s);
                    }
                    else
                    {
                        if (s.Start < extent.Start)
                        {
                            newDirtySpans.Add(Span.FromBounds(s.Start, extent.Start));
                        }

                        if ((s.End >= extent.End) && (extent.End < line.Snapshot.Length))
                        {
                            newDirtySpans.Add(Span.FromBounds(extent.End, s.End));  //This could add a zero length span (which is by design since we want to ensure we spell check the next line).
                        }
                    }
                }

                _dirtySpans = new NormalizedSnapshotSpanCollection(line.Snapshot, newDirtySpans);

                if (_dirtySpans.Count == 0)
                {
                    // We've cleaned up all the dirty spans.
                    _isUpdating = false;
                }
                else
                {
                    // Still more work to do.
                    _uiThreadDispatcher.BeginInvoke(new Action(() => this.DoUpdate()), DispatcherPriority.Background);
                }
            }
        }

        // Reject spelling errors for words that are probably code constructs embedded in a comment.
        // Reject if:
        //  Any upper case characters after the 1st character.
        //  Any _ in the span.
        //  Any . in the span.
        //  Any digits in the span.
        private static bool IsPossibleSpellingError(SnapshotSpan span)
        {
            for (int i = 0; (i < span.Length); ++i)
            {
                char c = (span.Start + i).GetChar();
                if ((c == '_') || (c == '.') || char.IsDigit(c) || ((i > 0) && char.IsUpper(c)))
                {
                    return false;
                }
            }

            return true;
        }

        private void UpdateSpellingErrors(SpellingErrorsSnapshot spellingErrors)
        {
            // Tell our factory to snap to a new snapshot.
            this.Factory.UpdateErrors(spellingErrors);

            // Tell the provider to mark all the sinks dirty (so, as a side-effect, they will start an update pass that will get the new snapshot
            // from the factory).
            _provider.UpdateAllSinks();

            foreach (var tagger in _activeTaggers)
            {
                tagger.UpdateErrors(_currentSnapshot, spellingErrors);
            }

            this.LastSpellingErrors = spellingErrors;
        }

        internal SpellingErrorsSnapshot LastSpellingErrors { get; private set; }
    }
}
