//------------------------------------------------------------------------------
// <copyright file="ToggleComment.cs" company="Microsoft">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Operations;
using OleInterop = Microsoft.VisualStudio.OLE.Interop;

namespace EditorCommands
{
    /// <summary>
    /// Command handler for ToggleComment
    /// </summary>
    internal sealed class ToggleComment
    {
        public static int HandleCommand(IWpfTextView textView, IClassifier classifier, OleInterop.IOleCommandTarget commandTarget, IEditorOperations editorOperations)
        {
            Guid cmdGroup = VSConstants.VSStd2K;

            // Is anything selected? (Or just a cursor)
            bool cursorOnly = IsCursorOnly(textView);

            // Execute Comment or Uncomment depending on current state of selected code
            uint cmdID = IsAllCommented(textView, classifier) ? (uint) VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK : (uint) VSConstants.VSStd2KCmdID.COMMENT_BLOCK;
            int hr = commandTarget.Exec(ref cmdGroup, cmdID, (uint)OleInterop.OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);

            if (cursorOnly)
            {
                // Move caret down one line
                editorOperations.MoveLineDown(extendSelection:false);
            }

            return VSConstants.S_OK;
        }

        private static bool IsCursorOnly(IWpfTextView textView)
        {
            if (textView.Selection.SelectedSpans.Count > 1) return false;
            // Only one selection. Check if there is any selected content.
            return textView.Selection.SelectedSpans[0].Length == 0;
        }

        private static bool IsAllCommented(IWpfTextView textView, IClassifier classifier)
        {
            foreach (SnapshotSpan snapshotSpan in textView.Selection.SelectedSpans)
            {
                SnapshotSpan spanToCheck = snapshotSpan.Length == 0 ?
                    new SnapshotSpan(textView.TextSnapshot, textView.Caret.ContainingTextViewLine.Extent.Span) :
                    snapshotSpan;
                IList<ClassificationSpan> classificationSpans = classifier.GetClassificationSpans(spanToCheck);
                foreach (var classification in classificationSpans)
                {
                    var name = classification.ClassificationType.Classification.ToLower();
                    if (!name.Contains(PredefinedClassificationTypeNames.Comment))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

    }
}
