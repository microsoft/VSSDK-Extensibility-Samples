/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{

    internal class HierarchyEventArgs : EventArgs {
        private uint itemId;
        private string fileName;
        private IVsTextLines buffer;

        public HierarchyEventArgs(uint itemId, string canonicalName) {
            this.itemId = itemId;
            this.fileName = canonicalName;
        }
        public string CanonicalName {
            get { return fileName; }
        }
        public uint ItemID {
            get { return itemId; }
        }
        public IVsTextLines TextBuffer {
            get { return buffer; }
            set { buffer = value; }
        }
    }

    internal class HierarchyListener : IVsHierarchyEvents, IDisposable {

        private IVsHierarchy hierarchy;
        private uint cookie;

        public HierarchyListener(IVsHierarchy hierarchy) {
            if (null == hierarchy) {
                throw new ArgumentNullException("hierarchy");
            }
            this.hierarchy = hierarchy;
        }

        #region Public Methods
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool IsListening {
            get { return (0 != cookie); }
        }
        public void StartListening(bool doInitialScan) {
            if (0 != cookie) {
                return;
            }
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                hierarchy.AdviseHierarchyEvents(this, out cookie));
            if (doInitialScan) {
                InternalScanHierarchy(VSConstants.VSITEMID_ROOT);
            }
        }
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void StopListening() {
            InternalStopListening(true);
        }
        #endregion

        #region IDisposable Members

        public void Dispose() {
            InternalStopListening(false);
            cookie = 0;
            hierarchy = null;
        }

        #endregion

        #region Public Events
        private EventHandler<HierarchyEventArgs> onItemAdded;
        public event EventHandler<HierarchyEventArgs> OnAddItem {
            add { onItemAdded += value; }
            remove { onItemAdded -= value; }
        }

        private EventHandler<HierarchyEventArgs> onItemDeleted;
        public event EventHandler<HierarchyEventArgs> OnDeleteItem {
            add { onItemDeleted += value; }
            remove { onItemDeleted -= value; }
        }

        #endregion

        #region IVsHierarchyEvents Members

        public int OnInvalidateIcon(IntPtr hicon) {
            // Do Nothing.
            return VSConstants.S_OK;
        }

        public int OnInvalidateItems(uint itemidParent) {
            // TODO: Find out if this event is needed.
            Debug.WriteLine("\n\tOnInvalidateItems\n");
            return VSConstants.S_OK;
        }

        public int OnItemAdded(uint itemidParent, uint itemidSiblingPrev, uint itemidAdded) {
            // Check if the item is a python file.
            Debug.WriteLine("\n\tOnItemAdded\n");
            string name;
            if (!IsPythonFile(itemidAdded, out name)) {
                return VSConstants.S_OK;
            }

            // This item is a python file, so we can notify that it is added to the hierarchy.
            if (null != onItemAdded) {
                HierarchyEventArgs args = new HierarchyEventArgs(itemidAdded, name);
                onItemAdded(hierarchy, args);
            }
            return VSConstants.S_OK;
        }

        public int OnItemDeleted(uint itemid) {
            Debug.WriteLine("\n\tOnItemDeleted\n");
            // Notify that the item is deleted only if it is a python file.
            string name;
            if (!IsPythonFile(itemid, out name)) {
                return VSConstants.S_OK;
            }
            if (null != onItemDeleted) {
                HierarchyEventArgs args = new HierarchyEventArgs(itemid, name);
                onItemDeleted(hierarchy, args);
            }
            return VSConstants.S_OK;
        }

        public int OnItemsAppended(uint itemidParent) {
            // TODO: Find out what this event is about.
            Debug.WriteLine("\n\tOnItemsAppended\n");
            return VSConstants.S_OK;
        }

        public int OnPropertyChanged(uint itemid, int propid, uint flags) {
            // Do Nothing.
            return VSConstants.S_OK;
        }

        #endregion

        private bool InternalStopListening(bool throwOnError) {
            if ((null != hierarchy) || (0 == cookie)) {
                return false;
            }
            int hr = hierarchy.UnadviseHierarchyEvents(cookie);
            if (throwOnError) {
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
            }
            cookie = 0;
            return Microsoft.VisualStudio.ErrorHandler.Succeeded(hr);
        }

        private bool IsPythonFile(uint itemId, out string canonicalName) {
            // Find out if this item is a physical file.
            Guid typeGuid = Guid.Empty;
            canonicalName = null;
            int hr = VSConstants.S_OK;
            try
            {
                hr = hierarchy.GetGuidProperty(itemId, (int)__VSHPROPID.VSHPROPID_TypeGuid, out typeGuid);
            }
            catch (System.Runtime.InteropServices.COMException) 
            {
                //For WPF Projects, they will throw an exception when trying to access this property if the
                //guid is empty. These are caught and ignored here.
            }

            if (Microsoft.VisualStudio.ErrorHandler.Failed(hr) ||
                VSConstants.GUID_ItemType_PhysicalFile != typeGuid) {
                // It is not a file, we can exit now.
                return false;
            }

            // This item is a file; find if it is a pyhon file.
            hr = hierarchy.GetCanonicalName(itemId, out canonicalName);
            if (Microsoft.VisualStudio.ErrorHandler.Failed(hr)) {
                return false;
            }
            string extension = System.IO.Path.GetExtension(canonicalName);
            return (0 == string.Compare(extension, IPyConstants.pythonFileExtension, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Do a recursive walk on the hierarchy to find all the python files in it.
        /// It will generate an event for every file found.
        /// </summary>
        private void InternalScanHierarchy(uint itemId) {
            uint currentItem = itemId;
            while (VSConstants.VSITEMID_NIL != currentItem) {
                // If this item is a python file, then send the add item event.
                string itemName;
                if ((null != onItemAdded) && IsPythonFile(currentItem, out itemName)) {
                    HierarchyEventArgs args = new HierarchyEventArgs(currentItem, itemName);
                    onItemAdded(hierarchy, args);
                }

                // NOTE: At the moment we skip the nested hierarchies, so here  we look for the 
                // children of this node.
                // Before looking at the children we have to make sure that the enumeration has not
                // side effects to avoid unexpected behavior.
                object propertyValue;
                bool canScanSubitems = true;
                int hr = hierarchy.GetProperty(currentItem, (int)__VSHPROPID.VSHPROPID_HasEnumerationSideEffects, out propertyValue);
                if ((VSConstants.S_OK == hr) && (propertyValue is bool)) {
                    canScanSubitems = !(bool)propertyValue;
                }
                // If it is allow to look at the sub-items of the current one, lets do it.
                if (canScanSubitems) {
                    object child;
                    hr = hierarchy.GetProperty(currentItem, (int)__VSHPROPID.VSHPROPID_FirstChild, out child);
                    if (VSConstants.S_OK == hr) {
                        // There is a sub-item, call this same function on it.
                        InternalScanHierarchy(GetItemId(child));
                    }
                }

                // Move the current item to its first visible sibling.
                object sibling;
                hr = hierarchy.GetProperty(currentItem, (int)__VSHPROPID.VSHPROPID_NextSibling, out sibling);
                if (VSConstants.S_OK != hr) {
                    currentItem = VSConstants.VSITEMID_NIL;
                } else {
                    currentItem = GetItemId(sibling);
                }
            }
        }

        /// <summary>
        /// Gets the item id.
        /// </summary>
        /// <param name="variantValue">VARIANT holding an itemid.</param>
        /// <returns>Item Id of the concerned node</returns>
        private static uint GetItemId(object variantValue)
        {
            if (variantValue == null) return VSConstants.VSITEMID_NIL;
            if (variantValue is int) return (uint)(int)variantValue;
            if (variantValue is uint) return (uint)variantValue;
            if (variantValue is short) return (uint)(short)variantValue;
            if (variantValue is ushort) return (uint)(ushort)variantValue;
            if (variantValue is long) return (uint)(long)variantValue;
            return VSConstants.VSITEMID_NIL;
        }
    }
}
