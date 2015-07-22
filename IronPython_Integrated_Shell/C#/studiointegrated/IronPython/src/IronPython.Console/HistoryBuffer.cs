/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;

namespace Microsoft.Samples.VisualStudio.IronPython.Console
{
    /// <summary>
    /// Implements the buffer used for the history in the console window.
    /// </summary>
    internal class HistoryBuffer
    {
        // Default size of the buffer. This is the size used when the buffer is
        // built without any parameter.
        internal const int defaultBufferSize = 20;

        // The array of strings that stores the history of the commands.
        private List<string> buffer;
        // Current position inside the buffer.
        private int currentPosition;
        // Flag true if the current item was returned.
        private bool currentReturned;

        /// <summary>
        /// Creates an HistoryBuffer object with the default size.
        /// </summary>
        public HistoryBuffer() :
            this(defaultBufferSize)
        { }

        /// <summary>
        /// Creates a HistoryBuffer object of a specifuc size.
        /// </summary>
        public HistoryBuffer(int bufferSize)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }
            buffer = new List<string>(bufferSize);
        }

        /// <summary>
        /// Search in the buffer if there is an item and returns its index.
        /// Returns -1 if the item is not in the buffer.
        /// </summary>
        private int FindIndex(string entry)
        {
            for (int i = 0; i < buffer.Count; i++)
            {
                if (string.CompareOrdinal(entry, buffer[i]) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Add a new entry in the list.
        /// </summary>
        public void AddEntry(string entry)
        {
            // Check if this entry is in the buffer.
            int index = FindIndex(entry);
            currentReturned = false;
            if (-1 != index)
            {
                // The index is in the buffer, so set the it as the current one
                // and return.
                currentPosition = index;
                return;
            }
            // The entry is not in the buffer, so we have to add it. 
            // Before add the new item we have to check if the buffer is over
            // capacity.
            if (buffer.Count == buffer.Capacity)
            {
                // Remove the first element in the buffer.
                buffer.RemoveAt(0);
            }
            // Add the new entry at the end of the buffer.
            buffer.Add(entry);
            // Set the current position at the end of the buffer.
            currentPosition = buffer.Count - 1;
        }

        /// <summary>
        /// Returns the previous element in the history or null if there is no
        /// previous entry.
        /// </summary>
        public string PreviousEntry()
        {
            if ((buffer.Count == 0) || (currentPosition < 0))
            {
                return null;
            }
            if (!currentReturned)
            {
                currentReturned = true;
                return buffer[currentPosition];
            }
            currentPosition -= 1;
            if (currentPosition < 0)
            {
                currentPosition = 0;
                return null;
            }
            return buffer[currentPosition];
        }

        /// <summary>
        /// Return the next entry in the history or null if there is no entry.
        /// </summary>
        public string NextEntry()
        {
            if (currentPosition >= buffer.Count - 1)
            {
                return null;
            }
            currentReturned = true;
            currentPosition += 1;
            return buffer[currentPosition];
        }
    }
}
