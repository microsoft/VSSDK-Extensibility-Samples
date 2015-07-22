/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.Samples.VisualStudio.IronPython.Project.Library
{

    /// <summary>
    /// Single node inside the tree of the libraries in the object browser or class view.
    /// </summary>
    internal class LibraryNode : IVsSimpleObjectList2, IVsNavInfoNode {

        public const uint NullIndex = (uint)0xFFFFFFFF;

        /// <summary>
        /// Enumeration of the capabilities of a node. It is possible to combine different values
        /// to support more capabilities.
        /// This enumeration is a copy of _LIB_LISTCAPABILITIES with the Flags attribute set.
        /// </summary>
        [Flags()]
        public enum LibraryNodeCapabilities {
            None = _LIB_LISTCAPABILITIES.LLC_NONE,
            HasBrowseObject = _LIB_LISTCAPABILITIES.LLC_HASBROWSEOBJ,
            HasDescriptionPane = _LIB_LISTCAPABILITIES.LLC_HASDESCPANE,
            HasSourceContext = _LIB_LISTCAPABILITIES.LLC_HASSOURCECONTEXT,
            HasCommands = _LIB_LISTCAPABILITIES.LLC_HASCOMMANDS,
            AllowDragDrop = _LIB_LISTCAPABILITIES.LLC_ALLOWDRAGDROP,
            AllowRename = _LIB_LISTCAPABILITIES.LLC_ALLOWRENAME,
            AllowDelete = _LIB_LISTCAPABILITIES.LLC_ALLOWDELETE,
            AllowSourceControl = _LIB_LISTCAPABILITIES.LLC_ALLOWSCCOPS,
        }

        /// <summary>
        /// Enumeration of the possible types of node. The type of a node can be the combination
        /// of one of more of these values.
        /// This is actually a copy of the _LIB_LISTTYPE enumeration with the difference that the
        /// Flags attribute is set so that it is possible to specify more than one value.
        /// </summary>
        [Flags()]
        public enum LibraryNodeType {
            None = 0,
            Hierarchy = _LIB_LISTTYPE.LLT_HIERARCHY,
            Namespaces = _LIB_LISTTYPE.LLT_NAMESPACES,
            Classes = _LIB_LISTTYPE.LLT_CLASSES,
            Members = _LIB_LISTTYPE.LLT_MEMBERS,
            Package = _LIB_LISTTYPE.LLT_PACKAGE,
            PhysicalContainer = _LIB_LISTTYPE.LLT_PHYSICALCONTAINERS,
            Containment = _LIB_LISTTYPE.LLT_CONTAINMENT,
            ContainedBy = _LIB_LISTTYPE.LLT_CONTAINEDBY,
            UsesClasses = _LIB_LISTTYPE.LLT_USESCLASSES,
            UsedByClasses = _LIB_LISTTYPE.LLT_USEDBYCLASSES,
            NestedClasses = _LIB_LISTTYPE.LLT_NESTEDCLASSES,
            InheritedInterface = _LIB_LISTTYPE.LLT_INHERITEDINTERFACES,
            InterfaceUsedByClasses = _LIB_LISTTYPE.LLT_INTERFACEUSEDBYCLASSES,
            Definitions = _LIB_LISTTYPE.LLT_DEFINITIONS,
            References = _LIB_LISTTYPE.LLT_REFERENCES,
            DeferExpansion = _LIB_LISTTYPE.LLT_DEFEREXPANSION,
        }

        private string name;
        private LibraryNodeType type;
        private List<LibraryNode> children;
        private LibraryNodeCapabilities capabilities;
        private List<VSOBJCLIPFORMAT> clipboardFormats;
        public VSTREEDISPLAYDATA displayData;
        private _VSTREEFLAGS flags;
        private CommandID contextMenuID;
        private string tooltip;
        private uint updateCount;
        private Dictionary<LibraryNodeType, LibraryNode> filteredView;

        public LibraryNode(string name)
            : this(name, LibraryNodeType.None, LibraryNodeCapabilities.None, null)
        { }
        public LibraryNode(string name, LibraryNodeType type)
            : this(name, type, LibraryNodeCapabilities.None, null)
        { }
        public LibraryNode(string name, LibraryNodeType type, LibraryNodeCapabilities capabilities, CommandID contextMenuID) {
            this.capabilities = capabilities;
            this.contextMenuID = contextMenuID;
            this.name = name;
            this.tooltip = name;
            this.type = type;
            children = new List<LibraryNode>();
            clipboardFormats = new List<VSOBJCLIPFORMAT>();
            filteredView = new Dictionary<LibraryNodeType, LibraryNode>();
        }
        public LibraryNode(LibraryNode node) {
            this.capabilities = node.capabilities;
            this.contextMenuID = node.contextMenuID;
            this.displayData = node.displayData;
            this.name = node.name;
            this.tooltip = node.tooltip;
            this.type = node.type;
            this.children = new List<LibraryNode>();
            foreach (LibraryNode child in node.children) {
                children.Add(child);
            }
            this.clipboardFormats = new List<VSOBJCLIPFORMAT>();
            foreach (VSOBJCLIPFORMAT format in node.clipboardFormats) {
                clipboardFormats.Add(format);
            }
            this.filteredView = new Dictionary<LibraryNodeType, LibraryNode>();
            this.updateCount = node.updateCount;
        }

        protected void SetCapabilityFlag(LibraryNodeCapabilities flag, bool value) {
            if (value) {
                capabilities |= flag;
            } else {
                capabilities &= ~flag;
            }
        }

        /// <summary>
        /// Get or Set if the node can be deleted.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool CanDelete {
            get { return (0 != (capabilities & LibraryNodeCapabilities.AllowDelete)); }
            set { SetCapabilityFlag(LibraryNodeCapabilities.AllowDelete, value); }
        }

        /// <summary>
        /// Get or Set if the node can be associated with some source code.
        /// </summary>
        public bool CanGoToSource {
            get { return (0 != (capabilities & LibraryNodeCapabilities.HasSourceContext)); }
            set { SetCapabilityFlag(LibraryNodeCapabilities.HasSourceContext, value); }
        }

        /// <summary>
        /// Get or Set if the node can be renamed.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool CanRename {
            get { return (0 != (capabilities & LibraryNodeCapabilities.AllowRename)); }
            set { SetCapabilityFlag(LibraryNodeCapabilities.AllowRename, value); }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public LibraryNodeCapabilities Capabilities {
            get { return capabilities; }
            set { capabilities = value; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public _VSTREEFLAGS Flags {
            get { return flags; }
            set { flags = value; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string TooltipText {
            get { return tooltip; }
            set { tooltip = value; }
        }

        internal void AddNode(LibraryNode node) {
            lock (children) {
                children.Add(node);
            }
            updateCount += 1;
        }
        internal void RemoveNode(LibraryNode node) {
            lock (children) {
                children.Remove(node);
            }
            updateCount += 1;
        }

        protected virtual object BrowseObject {
            get { return null; }
        }

        protected virtual uint CategoryField(LIB_CATEGORY category) {
            uint fieldValue = 0;
            switch (category) {
                case LIB_CATEGORY.LC_LISTTYPE:
                    {
                        LibraryNodeType subTypes = LibraryNodeType.None;
                        foreach (LibraryNode node in children) {
                            subTypes |= node.type;
                        }
                        fieldValue = (uint)subTypes;
                    }
                    break;
                case (LIB_CATEGORY)_LIB_CATEGORY2.LC_HIERARCHYTYPE:
                    fieldValue = (uint)_LIBCAT_HIERARCHYTYPE.LCHT_UNKNOWN;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return fieldValue;
        }

        protected virtual LibraryNode Clone() {
            return new LibraryNode(this);
        }

        /// <summary>
        /// Performs the operations needed to delete this node.
        /// </summary>
        protected virtual void Delete() {
        }

        /// <summary>
        /// Perform a Drag and Drop operation on this node.
        /// </summary>
        protected virtual void DoDragDrop(OleDataObject dataObject, uint keyState, uint effect) {
        }

        protected virtual uint EnumClipboardFormats(_VSOBJCFFLAGS flagsArg, VSOBJCLIPFORMAT[] formats) {
            if ((null == formats) || (formats.Length == 0)) {
                return (uint)clipboardFormats.Count;
            }
            uint itemsToCopy = (uint)clipboardFormats.Count;
            if (itemsToCopy > (uint)formats.Length) {
                itemsToCopy = (uint)formats.Length;
            }
            Array.Copy(clipboardFormats.ToArray(), formats, (int)itemsToCopy);
            return itemsToCopy;
        }

        protected virtual void FillDescription(_VSOBJDESCOPTIONS flagsArg, IVsObjectBrowserDescription3 description) {
            description.ClearDescriptionText();
            description.AddDescriptionText3(name, VSOBDESCRIPTIONSECTION.OBDS_NAME, null);
        }

        protected IVsSimpleObjectList2 FilterView(LibraryNodeType filterType) {
            LibraryNode filtered = null;
            if (filteredView.TryGetValue(filterType, out filtered)) {
                return filtered as IVsSimpleObjectList2;
            }
            filtered = this.Clone();
            for (int i = 0; i < filtered.children.Count;) {
                if (0 == (filtered.children[i].type & filterType)) {
                    filtered.children.RemoveAt(i);
                } else {
                    i += 1;
                }
            }
            filteredView.Add(filterType, filtered);
            return filtered as IVsSimpleObjectList2;
        }

        protected virtual void GotoSource(VSOBJGOTOSRCTYPE gotoType) {
            // Do nothing.
        }

        public string Name {
            get { return name; }
            set { name = value; }
        }

        public LibraryNodeType NodeType {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Finds the source files associated with this node.
        /// </summary>
        /// <param name="hierarchy">The hierarchy containing the items.</param>
        /// <param name="itemId">The item id of the item.</param>
        /// <param name="itemsCount">Number of items.</param>
        protected virtual void SourceItems(out IVsHierarchy hierarchy, out uint itemId, out uint itemsCount) {
            hierarchy = null;
            itemId = 0;
            itemsCount = 0;
        }

        protected virtual void Rename(string newName, uint flagsArg) {
            this.name = newName;
        }

        public virtual string UniqueName {
            get { return Name; }
        }

        #region IVsSimpleObjectList2 Members

        int IVsSimpleObjectList2.CanDelete(uint index, out int pfOK) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            pfOK = children[(int)index].CanDelete ? 1 : 0;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.CanGoToSource(uint index, VSOBJGOTOSRCTYPE SrcType, out int pfOK) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            pfOK = children[(int)index].CanGoToSource ? 1 : 0;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.CanRename(uint index, string pszNewName, out int pfOK) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            pfOK = children[(int)index].CanRename ? 1 : 0;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.CountSourceItems(uint index, out IVsHierarchy ppHier, out uint pItemid, out uint pcItems) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            children[(int)index].SourceItems(out ppHier, out pItemid, out pcItems);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.DoDelete(uint index, uint grfFlags) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            children[(int)index].Delete();
            children.RemoveAt((int)index);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.DoDragDrop(uint index, IDataObject pDataObject, uint grfKeyState, ref uint pdwEffect) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            OleDataObject dataObject = new OleDataObject(pDataObject);
            children[(int)index].DoDragDrop(dataObject, grfKeyState, pdwEffect);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.DoRename(uint index, string pszNewName, uint grfFlags) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            children[(int)index].Rename(pszNewName, grfFlags);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.EnumClipboardFormats(uint index, uint grfFlags, uint celt, VSOBJCLIPFORMAT[] rgcfFormats, uint[] pcActual) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            uint copied = children[(int)index].EnumClipboardFormats((_VSOBJCFFLAGS)grfFlags, rgcfFormats);
            if ((null != pcActual) && (pcActual.Length > 0)) {
                pcActual[0] = copied;
            }
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.FillDescription2(uint index, uint grfOptions, IVsObjectBrowserDescription3 pobDesc) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            children[(int)index].FillDescription((_VSOBJDESCOPTIONS)grfOptions, pobDesc);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetBrowseObject(uint index, out object ppdispBrowseObj) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            ppdispBrowseObj = children[(int)index].BrowseObject;
            if (null == ppdispBrowseObj) {
                return VSConstants.E_NOTIMPL;
            }
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetCapabilities2(out uint pgrfCapabilities) {
            pgrfCapabilities = (uint)Capabilities;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetCategoryField2(uint index, int Category, out uint pfCatField) {
            LibraryNode node;
            if (NullIndex == index) {
                node = this;
            } else if (index < (uint)children.Count) {
                node = children[(int)index];
            } else {
                throw new ArgumentOutOfRangeException("index");
            }
            pfCatField = node.CategoryField((LIB_CATEGORY)Category);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetClipboardFormat(uint index, uint grfFlags, FORMATETC[] pFormatetc, STGMEDIUM[] pMedium) {
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GetContextMenu(uint index, out Guid pclsidActive, out int pnMenuId, out IOleCommandTarget ppCmdTrgtActive) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            CommandID commandId = children[(int)index].contextMenuID;
            if (null == commandId) {
                pclsidActive = Guid.Empty;
                pnMenuId = 0;
                ppCmdTrgtActive = null;
                return VSConstants.E_NOTIMPL;
            }
            pclsidActive = commandId.Guid;
            pnMenuId = commandId.ID;
            ppCmdTrgtActive = children[(int)index] as IOleCommandTarget;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetDisplayData(uint index, VSTREEDISPLAYDATA[] pData) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            pData[0] = children[(int)index].displayData;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetExpandable3(uint index, uint ListTypeExcluded, out int pfExpandable) {
            // There is a not empty implementation of GetCategoryField2, so this method should
            // return E_NOTIMPL.
            pfExpandable = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GetExtendedClipboardVariant(uint index, uint grfFlags, VSOBJCLIPFORMAT[] pcfFormat, out object pvarFormat) {
            pvarFormat = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GetFlags(out uint pFlags) {
            pFlags = (uint)Flags;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetItemCount(out uint pCount) {
            pCount = (uint)children.Count;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetList2(uint index, uint ListType, uint flagsArg, VSOBSEARCHCRITERIA2[] pobSrch, out IVsSimpleObjectList2 ppIVsSimpleObjectList2) {
            // TODO: Use the flags and list type to actually filter the result.
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            ppIVsSimpleObjectList2 = children[(int)index].FilterView((LibraryNodeType)ListType);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetMultipleSourceItems(uint index, uint grfGSI, uint cItems, VSITEMSELECTION[] rgItemSel) {
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GetNavInfo(uint index, out IVsNavInfo ppNavInfo) {
            ppNavInfo = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GetNavInfoNode(uint index, out IVsNavInfoNode ppNavInfoNode) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            ppNavInfoNode = children[(int)index] as IVsNavInfoNode;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetProperty(uint index, int propid, out object pvar) {
            pvar = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GetSourceContextWithOwnership(uint index, out string pbstrFilename, out uint pulLineNum) {
            pbstrFilename = null;
            pulLineNum = (uint)0;
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GetTextWithOwnership(uint index, VSTREETEXTOPTIONS tto, out string pbstrText) {
            // TODO: make use of the text option.
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            pbstrText = children[(int)index].name;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetTipTextWithOwnership(uint index, VSTREETOOLTIPTYPE eTipType, out string pbstrText) {
            // TODO: Make use of the tooltip type.
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            pbstrText = children[(int)index].TooltipText;
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.GetUserContext(uint index, out object ppunkUserCtx) {
            ppunkUserCtx = null;
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.GoToSource(uint index, VSOBJGOTOSRCTYPE SrcType) {
            if (index >= (uint)children.Count) {
                throw new ArgumentOutOfRangeException("index");
            }
            children[(int)index].GotoSource(SrcType);
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.LocateNavInfoNode(IVsNavInfoNode pNavInfoNode, out uint pulIndex) {
            if (null == pNavInfoNode) {
                throw new ArgumentNullException("pNavInfoNode");
            }
            pulIndex = NullIndex;
            string nodeName;
            ErrorHandler.ThrowOnFailure(pNavInfoNode.get_Name(out nodeName));
            for (int i = 0; i < children.Count; i++ ) {
                if (0 == string.Compare(children[i].UniqueName, nodeName, StringComparison.OrdinalIgnoreCase)) {
                    pulIndex = (uint)i;
                    return VSConstants.S_OK;
                }
            }
            return VSConstants.S_FALSE;
        }

        int IVsSimpleObjectList2.OnClose(VSTREECLOSEACTIONS[] ptca) {
            // Do Nothing.
            return VSConstants.S_OK;
        }

        int IVsSimpleObjectList2.QueryDragDrop(uint index, IDataObject pDataObject, uint grfKeyState, ref uint pdwEffect) {
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.ShowHelp(uint index) {
            return VSConstants.E_NOTIMPL;
        }

        int IVsSimpleObjectList2.UpdateCounter(out uint pCurUpdate) {
            pCurUpdate = updateCount;
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsNavInfoNode Members

        int IVsNavInfoNode.get_Name(out string pbstrName) {
            pbstrName = UniqueName;
            return VSConstants.S_OK;
        }

        int IVsNavInfoNode.get_Type(out uint pllt) {
            pllt = (uint)type;
            return VSConstants.S_OK;
        }

        #endregion
    }
}
