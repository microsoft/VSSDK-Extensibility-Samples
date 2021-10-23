/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace JoinLineCommandImplementation
{
    public static class JoinLine
    {
        public static void JoinSelectedLines(ITextView textView, IEditorOperations editorOperations)
        {
            if (textView.Selection.IsEmpty)
            {
                return;
            }

            var selectedSpan = textView.Selection.SelectedSpans[0];
            textView.TextBuffer.Replace(selectedSpan, selectedSpan.GetText().Replace("\r\n", " "));

            editorOperations.MoveToEndOfLine(extendSelection: false);
            editorOperations.Delete();
            editorOperations.DeleteHorizontalWhiteSpace();
        }
    }
}
