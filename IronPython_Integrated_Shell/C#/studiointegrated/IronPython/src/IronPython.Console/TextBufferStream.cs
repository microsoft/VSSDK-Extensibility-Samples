/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Samples.VisualStudio.IronPython.Console
{
    /// <summary>
    /// This class implements a Stream on top of a text buffer.
    /// </summary>
    internal class TextBufferStream : Stream
    {
        // The text buffer used to write.
        private ITextBuffer textBuffer;
        // The text marker used to mark the read-only region of the buffer.
        private byte[] byteBuffer;
        int usedBuffer;

        private const int bufferSize = 1024;

        /// <summary>
        /// Creates a new TextBufferStream on top of a text buffer.
        /// The optional text marker can be used to let the stream set read only the
        /// text that it writes on the buffer.
        /// </summary>
        public TextBufferStream(ITextBuffer buffer)
        {
            if (null == buffer)
            {
                throw new ArgumentNullException("lines");
            }
            textBuffer = buffer;
            byteBuffer = new byte[bufferSize];
        }

        /// <summary>
        /// Gets the read status of the stream.
        /// </summary>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the seek status of the stream.
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the write status of the stream.
        /// </summary>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Flushes the pending data.
        /// </summary>
        public override void Flush()
        {
            // If there is no data in the buffer, then there is nothing to do.
            if (0 == usedBuffer)
            {
                // Make sure that the read-only region is correct and exit.
                ExtendReadOnlyRegion();
                return;
            }

            string text = null;
            // We have to use a StreamReader in order to work around problems with the
            // encoding of the data sent in, but in order to build the reader we need
            // a memory stream to read the data in the buffer.
            using (MemoryStream s = new MemoryStream(byteBuffer, 0, usedBuffer))
            {
                // Now we can build the reader from the memory stream.
                using (StreamReader reader = new StreamReader(s))
                {
                    // At the end we can get the text.
                    text = reader.ReadToEnd();
                }
            }
            // Now the buffer is empty.
            usedBuffer = 0;

            if (!textBuffer.EditInProgress)
            {
                var edit = textBuffer.CreateEdit();
                edit.Insert(textBuffer.CurrentSnapshot.Length, text);
                edit.Apply();
            }

            ExtendReadOnlyRegion();
        }

        /// <summary>
        /// Gets the size of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                return textBuffer.CurrentSnapshot.Length;
            }
        }

        /// <summary>
        /// Gets the current position inside the stream. The set function is not implemented.
        /// </summary>
        public override long Position
        {
            get
            {
                // The position is always at the end of the buffer.
                return textBuffer.CurrentSnapshot.Length;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Reads data from the stream. This function is not implemented.
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Seeks for a specific position inside the stream. This function is
        /// not implemented.
        /// </summary>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the length of the stream. This function is not implemented.
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes some data in the stream.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (null == buffer)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset >= buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || (offset + count > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            int totalCopied = 0;
            while (totalCopied < count)
            {
                int copySize = Math.Min(byteBuffer.Length - usedBuffer, count - totalCopied);
                if (copySize > 0)
                    System.Array.Copy(buffer, offset, byteBuffer, usedBuffer, copySize);
                usedBuffer += copySize;
                if (usedBuffer >= byteBuffer.Length)
                {
                    Flush();
                }
                totalCopied += copySize;
            }
        }

        /// <summary>
        /// Expands the read only region to the end of the current buffer
        /// </summary>
        private void ExtendReadOnlyRegion()
        {
            if (!textBuffer.EditInProgress)
            {
                if (readOnlyRegion != null)
                {
                    var readOnlyRemove = textBuffer.CreateReadOnlyRegionEdit();
                    readOnlyRemove.RemoveReadOnlyRegion(readOnlyRegion);
                    readOnlyRemove.Apply();
                }
                var readOnlyEdit = textBuffer.CreateReadOnlyRegionEdit();
                readOnlyRegion = readOnlyEdit.CreateReadOnlyRegion(new Span(0, textBuffer.CurrentSnapshot.Length));
                readOnlyEdit.Apply();
            }
        }

        private IReadOnlyRegion readOnlyRegion;

        internal void ClearReadOnlyRegion()
        {
            var readOnlyRemove = textBuffer.CreateReadOnlyRegionEdit();
            readOnlyRemove.RemoveReadOnlyRegion(readOnlyRegion);
            readOnlyRemove.Apply();
            readOnlyRegion = null;

            var edit = textBuffer.CreateEdit();
            edit.Delete(new Span(0, textBuffer.CurrentSnapshot.Length));//, string.Empty);
            edit.Insert(0, Resources.DefaultConsolePrompt);
            edit.Apply();

            ExtendReadOnlyRegion();
        }
    }
}
