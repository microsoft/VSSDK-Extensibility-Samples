/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    class TaskEnumerator : IVsEnumTaskItems
    {
        /// <summary>
        /// Creates a new task enumerator.
        /// </summary>
        /// <param name="items">The items this enumerator will enumerate.</param>
        /// <param name="showIgnored">Determines whether tasks for which Ignored==true will be skipped.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>items</c> is null.</exception>
        /// <remarks>
        /// This enumerator operates on a copy of the contents of <c>items</c>, so if <c>items</c>
        /// is changed after this call, it will not affect the results.
        /// </remarks>
        public TaskEnumerator(IEnumerable<Task> items, bool showIgnored)
        {
            _showIgnored = showIgnored;
            _items = new List<Task>(items);
        }

        #region IVsEnumTaskItems Members

        /// <summary>
        /// Creates a new enumerator with identical content to this one.
        /// </summary>
        /// <param name="ppenum">The newly created enumerator.</param>
        /// <returns>An HRESULT indicating the success of the operation.</returns>
        public int Clone(out IVsEnumTaskItems ppenum)
        {
            ppenum = new TaskEnumerator(_items, _showIgnored);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Enumerates a requested number of items.
        /// </summary>
        /// <param name="celt">The number of items to return.</param>
        /// <param name="rgelt">The array of items that will be returned.</param>
        /// <param name="pceltFetched">The array whose first element will be set to the actual number of items returned.</param>
        /// <returns>S_OK if all requested items were returned; S_FALSE if fewer were available; E_INVALIDARG if <c>celt</c> is less than zero, <c>rgelt</c> is null, or <c>pceltFetched</c> is null.</returns>
        /// <remarks>
        /// This method returns failure codes instead of throwing exceptions because it is intended to be called from native code.
        /// If the task provider's IsShowingIgnoredInstances property is false, ignored instances will be skipped.
        /// </remarks>
        public int Next(uint celt, IVsTaskItem[] rgelt, uint[] pceltFetched)
        {
            pceltFetched[0] = 0;

            while (pceltFetched[0] < celt && _next < _items.Count)
            {
                if (_showIgnored || !_items[_next].Ignored)
                {
                    rgelt[pceltFetched[0]] = _items[_next];
                    pceltFetched[0]++;
                }
                ++_next;
            }

            if (pceltFetched[0] == celt)
            {
                return VSConstants.S_OK;
            }
            else
            {
                return VSConstants.S_FALSE;
            }
        }

        /// <summary>
        /// Resets the enumerator so that the next call to <c>Next</c> will begin at the first element.
        /// </summary>
        /// <returns>S_OK</returns>
        public int Reset()
        {
            _next = 0;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Skips a specified number of items
        /// </summary>
        /// <param name="celt">The number of items to skip.</param>
        /// <returns>S_OK if the requested number of items was skipped; S_FALSE if <c>celt</c> is larger than the number of available items; E_INVALIDARG if <c>celt</c> is less than zero.</returns>
        /// <remarks>
        /// This method returns failure codes instead of throwing exceptions because it is intended to be called from native code.
        /// If the task provider's IsShowingIgnoredInstances property is false, ignored instances will not be counted.
        /// </remarks>
        public int Skip(uint celt)
        {
            IVsTaskItem[] items = new IVsTaskItem[celt];
            uint[] fetched = new uint[] { 0 };

            return Next(celt, items, fetched);
        }

        #endregion IVsEnumTaskItems Members

        #region Private Members

        readonly List<Task> _items;
        int _next = 0;
        readonly bool _showIgnored;

        #endregion Private Members
    }
}
