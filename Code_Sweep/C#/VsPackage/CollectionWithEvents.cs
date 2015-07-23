/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    class ItemEventArgs<T> : EventArgs
    {
        readonly T _item;

        public ItemEventArgs(T item)
        {
            _item = item;
        }

        T Item
        {
            get { return _item; }
        }
    }

    class CollectionWithEvents<T> : ICollection<T>
    {
        readonly List<T> _list = new List<T>();

        /// <summary>
        /// Fired once for each item that is added to the collection, after the item is added.
        /// </summary>
        public event EventHandler<ItemEventArgs<T>> ItemAdded;

        /// <summary>
        /// Fired once for each item that is removed from the collection, before the item is removed.
        /// </summary>
        public event EventHandler<ItemEventArgs<int>> ItemRemoving;

        /// <summary>
        /// Fired once for each action that removes one or more items from the collection, after the items have been removed.
        /// </summary>
        public event EventHandler ItemsRemoved;

        public void AddRange(IEnumerable<T> content)
        {
            foreach (T t in content)
            {
                Add(t);
            }
        }

        #region ICollection<T> Members

        public void Add(T item)
        {
            _list.Add(item);
            var handler = ItemAdded;
            if (handler != null)
            {
                handler(this, new ItemEventArgs<T>(item));
            }
        }

        public void Clear()
        {
            FireItemRemoving(0, Count);
            _list.Clear();
            FireItemsRemoved();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            int index = _list.IndexOf(item);
            if (index < 0)
                return false;

            FireItemRemoving(index);
            _list.RemoveAt(index);
            FireItemsRemoved();
            return true;
        }

        #endregion ICollection<T> Members

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion IEnumerable<T> Members

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion IEnumerable Members

        #region Private Members

        void FireItemRemoving(int firstIndex, int count = 1)
        {
            var handler = ItemRemoving;
            if (handler != null)
            {
                for (int i = firstIndex; i < firstIndex + count; ++i)
                {
                    handler(this, new ItemEventArgs<int>(i));
                }
            }
        }

        void FireItemsRemoved()
        {
            var handler = ItemsRemoved;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion Private Members
    }
}
