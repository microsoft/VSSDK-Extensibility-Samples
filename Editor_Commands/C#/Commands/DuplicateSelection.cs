using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using IServiceProvider = System.IServiceProvider;

namespace EditorCommands
{
    internal sealed class DuplicateSelection
    {
        private readonly Package _package;

        public static DuplicateSelection Instance { get; private set; }

        private IServiceProvider ServiceProvider => _package;

        public static void Initialize(Package package)
        {
            Instance = new DuplicateSelection(package);
        }

        private DuplicateSelection(Package package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));
            _package = package;
        }
        // Helped by source of Microsoft.VisualStudio.Text.Editor.DragDrop.DropHandlerBase.cs in assembly Microsoft.VisualStudio.Text.UI.Wpf, Version=14.0.0.0
        public static int HandleCommand(IWpfTextView textView, IClassifier classifier, IOleCommandTarget commandTarget, IEditorOperations editorOperations, bool shiftPressed = false)
        {
            //Guid cmdGroup = VSConstants.VSStd2K;
            var isSingleLine = false;
            var selectedText = editorOperations.SelectedText;
            ITrackingPoint trackingPoint = null;
            if (selectedText.Length == 0)
            // if nothing is selected, we can consider the current line as a selection
            {
                var virtualBufferPosition = editorOperations.TextView.Caret.Position.VirtualBufferPosition;
                trackingPoint = textView.TextSnapshot.CreateTrackingPoint(virtualBufferPosition.Position, PointTrackingMode.Negative);
                editorOperations.SelectLine(textView.Caret.ContainingTextViewLine, false);
                isSingleLine = true;
            }

            if (isSingleLine)
            {
                editorOperations.CopySelection();
                editorOperations.MoveToNextCharacter(false);
                editorOperations.Paste();
                editorOperations.MoveToPreviousCharacter(false);

                textView.Caret.MoveTo(new VirtualSnapshotPoint(trackingPoint.GetPoint(textView.TextSnapshot)).TranslateTo(textView.TextSnapshot));
                if (!shiftPressed) editorOperations.MoveLineDown(false);
            }
            else
            {
                var selection = textView.Selection;
                var isReversed = selection.IsReversed;
                var text = selectedText;
                var textSnapshot = textView.TextSnapshot;
                var list = new List<ITrackingSpan>();
                //var shiftKeyPressed=textVie
                foreach (SnapshotSpan snapshotSpan in selection.SelectedSpans)
                {
                    list.Add(textSnapshot.CreateTrackingSpan(snapshotSpan, SpanTrackingMode.EdgeExclusive));
                }
                if (!selection.IsEmpty)
                {
                    selection.Clear();
                }


                if (list.Count < 2)
                {
                    var offset = 0;
                    var virtualBufferPosition = editorOperations.TextView.Caret.Position.VirtualBufferPosition;
                    var point = editorOperations.TextView.Caret.Position.BufferPosition;
                    virtualBufferPosition = isReversed && !shiftPressed ? new VirtualSnapshotPoint(point.Add(text.Length))
                       : !isReversed && shiftPressed ? new VirtualSnapshotPoint(point.Add(-text.Length)) : virtualBufferPosition;

                    trackingPoint = textSnapshot.CreateTrackingPoint(virtualBufferPosition.Position, PointTrackingMode.Negative);
                    if (virtualBufferPosition.IsInVirtualSpace)
                    {
                        offset = editorOperations.GetWhitespaceForVirtualSpace(virtualBufferPosition).Length;
                    }
                    textView.Caret.MoveTo(virtualBufferPosition.TranslateTo(textView.TextSnapshot));
                    editorOperations.InsertText(text);
                    var insertionPoint = trackingPoint.GetPoint(textView.TextSnapshot);
                    if (offset != 0)
                    {
                        insertionPoint = insertionPoint.Add(offset);
                    }

                    var virtualSnapshotPoint1 = new VirtualSnapshotPoint(insertionPoint);
                    var virtualSnapshotPoint2 = new VirtualSnapshotPoint(insertionPoint.Add(text.Length));
                    if (isReversed)
                    {
                        editorOperations.SelectAndMoveCaret(virtualSnapshotPoint2, virtualSnapshotPoint1, TextSelectionMode.Stream);
                    }
                    else
                    {
                        editorOperations.SelectAndMoveCaret(virtualSnapshotPoint1, virtualSnapshotPoint2, TextSelectionMode.Stream);
                    }
                }
                else
                {
                    var trackingPointOffsetList = new List<Tuple<ITrackingPoint, int, int>>();
                    //Insert Text!
                    if (isReversed) list.Reverse();
                    foreach (var trackingSpan in list)
                    {
                        var span = trackingSpan.GetSpan(textSnapshot);
                        text = trackingSpan.GetText(textSnapshot);
                        var offset = 0;
                        var insertionPoint = !isReversed ? trackingSpan.GetEndPoint(span.Snapshot) : trackingSpan.GetStartPoint(span.Snapshot);
                        var virtualBufferPosition = new VirtualSnapshotPoint(insertionPoint);
                        virtualBufferPosition = isReversed && !shiftPressed ? new VirtualSnapshotPoint(insertionPoint.Add(text.Length))
                           : !isReversed && shiftPressed ? new VirtualSnapshotPoint(insertionPoint.Add(-text.Length)) : virtualBufferPosition;


                        trackingPoint = textSnapshot.CreateTrackingPoint(virtualBufferPosition.Position, PointTrackingMode.Negative);
                        if (virtualBufferPosition.IsInVirtualSpace)
                        {
                            offset = editorOperations.GetWhitespaceForVirtualSpace(virtualBufferPosition).Length;
                        }
                        trackingPointOffsetList.Add(new Tuple<ITrackingPoint, int, int>(trackingPoint, offset, text.Length));
                        textView.Caret.MoveTo(virtualBufferPosition.TranslateTo(textView.TextSnapshot));
                        editorOperations.InsertText(text);
                    }
                    //Make Selections
                    {
                        var trackingPointOffset = trackingPointOffsetList.First();
                        var insertionPoint = trackingPointOffset.Item1.GetPoint(textView.TextSnapshot);
                        if (trackingPointOffset.Item2 != 0)
                        {
                            insertionPoint = insertionPoint.Add(trackingPointOffset.Item2);
                        }
                        var virtualSnapshotPoint1 = new VirtualSnapshotPoint(insertionPoint.Add(!isReversed ? 0 : trackingPointOffset.Item3));

                        trackingPointOffset = trackingPointOffsetList.Last();
                        insertionPoint = trackingPointOffset.Item1.GetPoint(textView.TextSnapshot);
                        if (trackingPointOffset.Item2 != 0)
                        {
                            insertionPoint = insertionPoint.Add(trackingPointOffset.Item2);
                        }
                        var virtualSnapshotPoint2 = new VirtualSnapshotPoint(insertionPoint.Add(isReversed ? 0 : trackingPointOffset.Item3));
                        editorOperations.SelectAndMoveCaret(virtualSnapshotPoint1, virtualSnapshotPoint2, TextSelectionMode.Box);
                    }
                }
            }

            return VSConstants.S_OK;
        }
    }
}
