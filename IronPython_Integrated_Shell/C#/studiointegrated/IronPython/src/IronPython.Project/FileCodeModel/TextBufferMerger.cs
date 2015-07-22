/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using IronPython.CodeDom;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {
    internal class TextBufferMerger : IMergeDestination {
        private bool hasMerged = false;
        private IVsTextLines textBuffer;
        internal TextBufferMerger(IVsTextLines buffer) {
            if (null == buffer) {
                throw new ArgumentNullException("buffer");
            }
            this.textBuffer = buffer;
        }

        public void InsertRange(int start, IList<string> lines) {
            // If the set of lines is empty we can exit now
            if ((null == lines) || (lines.Count == 0)) {
                hasMerged = true;
                return;
            }
            if (start < 0) {
                throw new ArgumentOutOfRangeException("start");
            }
            int insertLine = start;
            int insertIndex = 0;
            // Verify that the insertion point is inside the buffer.
            int totalLines = LineCount;
            if (insertLine > totalLines) {
                insertLine = totalLines;
            }
            // Create the text to add to the buffer.
            StringBuilder builder = new StringBuilder();
            if ((insertLine == totalLines) && (totalLines > 0)) {
                insertLine = totalLines - 1;
                ErrorHandler.ThrowOnFailure(textBuffer.GetLengthOfLine(insertLine, out insertIndex));
                builder.AppendLine();
            }
            foreach (string line in lines) {
                builder.AppendLine(line);
            }
            // Lock the buffer before changing its content.
            ErrorHandler.ThrowOnFailure(textBuffer.LockBuffer());
            try {
                // Get the text to insert and pin it so that we can pass it as pointer
                // to the buffer's functions.
                string text = builder.ToString();
                GCHandle handle = GCHandle.Alloc(text, GCHandleType.Pinned);
                try {
                    TextSpan[] span = new TextSpan[1];
                    ErrorHandler.ThrowOnFailure(textBuffer.ReplaceLines(insertLine, insertIndex, insertLine, insertIndex, handle.AddrOfPinnedObject(), text.Length, span));
                    hasMerged = true;
                } finally {
                    // Free the memory.
                    handle.Free();
                }
            } finally {
                // Make sure that the buffer is unlocked also in case of exception.
                textBuffer.UnlockBuffer();
            }
        }

        public void RemoveRange(int start, int count) {
            // Check if there is any line to remove.
            if (count <= 0) {
                hasMerged = true;
                return;
            }
            // Check if the index of the first line is correct.
            if (start < 0) {
                throw new ArgumentOutOfRangeException("start");
            }
            int startLine = start;
            // Find the number of lines in the buffer.
            int totalLines = LineCount;
            // If the start line if after the end of the buffer, then there is nothing to do.
            if (startLine >= totalLines) {
                hasMerged = true;
                return;
            }
            // Find the last line to remove.
            int endLine = startLine + count;
            int endIndex = 0;
            if (endLine >= totalLines) {
                ErrorHandler.ThrowOnFailure(textBuffer.GetLastLineIndex(out endLine, out endIndex));
            }
            // Lock the buffer.
            ErrorHandler.ThrowOnFailure(textBuffer.LockBuffer());
            try {
                // Remove the text replacing the lines with an empty string.
                TextSpan[] span = new TextSpan[1];
                ErrorHandler.ThrowOnFailure(textBuffer.ReplaceLines(startLine, 0, endLine, endIndex, IntPtr.Zero, 0, span));
                hasMerged = true;
            } finally {
                // unlock the buffer
                textBuffer.UnlockBuffer();
            }
        }

        public int LineCount {
            get {
                int lines;
                ErrorHandler.ThrowOnFailure(textBuffer.GetLineCount(out lines));
                return lines;
            }
        }

        public bool HasMerged {
            get { return hasMerged; }
        }

        public string FinalText {
            get { 
                // return back modified text
                hasMerged = false;
                return null;
            }
        }
    }
}
