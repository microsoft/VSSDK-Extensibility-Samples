/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel;
using System.Globalization;

namespace MyCompany.RdtEventExplorer
{
    public class GenericEvent
    {
        #region Member variables
        RunningDocumentInfo rdi;
        private string message;
        private string docNameShort;
        protected string docNameLong;
        
        // IVsRunningDocTableEvents
        protected uint grfAttribs;
        protected IVsWindowFrame pFrame;
        protected int fFirstShow;
        protected int fClosedWithoutSaving;
        protected uint dwRDTLockType;

        // IVsRunningDocTableEvents2
        private IVsHierarchy oldHierarchy;
        private IVsHierarchy newHierarchy;
        protected uint itemidOld;
        protected uint itemidNew;
        public string pszMkDocumentOld;
        public string pszMkDocumentNew;
        #endregion

        #region Constructors
        /// <summary>
        /// Base class for all other RDT event wrappers.  Each event wrapper 
        /// stores event-specific information and formats it for display
        /// in the Properties window.
        /// </summary>
        /// <param name="rdt">Running Document Table instance</param>
        /// <param name="message">Message to be displayed in the grid</param>
        /// <param name="cookie">Cookie to unadvise RDT events</param>
        public GenericEvent(RunningDocumentTable rdt, string message, uint cookie)
        {
            this.message = message;
            if (rdt == null || cookie == 0) return;

            rdi = rdt.GetDocumentInfo(cookie);
            docNameShort = GetShortDocName(rdi.Moniker);
        }
        #endregion
        #region Public properties
        [DisplayName("Event Name")]
        [Description("The name of the event.")]
        [Category("Basic")]
        public string EventName
        {
            get { return message; }
        }
        static char[] slashDelim = { '\\' };
        [DisplayName("Doc Name, Short")]
        [Description("The short name of the document.")]
        [Category("Basic")]
        public string DocumentName
        {
            get { return docNameShort; }
        }
        [DisplayName("Doc Name, Long")]
        [Description("The long name of the document.")]
        [Category("Basic")]
        public string DocumentMoniker
        {
            get { return rdi.Moniker; }
        }
        [DisplayName("Locks, Read")]
        [Description("The number of read locks.")]
        [Category("Basic")]
        public string RLock
        {
            get { return rdi.ReadLocks.ToString("d"); }
        }
        [DisplayName("Locks, Edit")]
        [Description("The number of edit locks.")]
        [Category("Basic")]
        public string ELock
        {
            get { return rdi.EditLocks.ToString("d", CultureInfo.CurrentCulture); }
        }
        protected IVsHierarchy OldHierarchy
        {
            get { return oldHierarchy; }
            set { oldHierarchy = value; }
        }
        protected IVsHierarchy NewHierarchy
        {
            get { return newHierarchy; }
            set { newHierarchy = value; }
        }
        #endregion

        #region Get and interpret
        /// <summary>
        /// Formats the file monikor for shortened display.  
        /// Guids are shortened for display, as are full paths.
        /// </summary>
        /// <param name="moniker">the file moniker associated with an event</param>
        /// <returns></returns>
        static protected string GetShortDocName(string moniker)
        {
            if (moniker == null) return String.Empty;
            // Handle GUID form.
            string name = moniker;
            int n = name.IndexOf("::{");
            if (n > -1)
            {
                return name.Substring(0, n + "::{".Length + 8) + "...}";
            }
            // Shorten name.
            string[] parts = name.Split(slashDelim);
            if (parts.Length < 1) return name;
            return parts[parts.Length - 1];
        }
        /// <summary>
        ///  Formats hierarchy item ID for display.
        /// </summary>
        /// <returns>Returns item ID formatted in hex.</returns>
        public string GetItemidOld()
        {
            return string.Format(CultureInfo.CurrentCulture, "0x{0:X}", itemidOld); 
        }
        /// <summary>
        ///  Formats hierarchy item ID for display.
        /// </summary>
        /// <returns>Returns item ID formatted in hex.</returns>
        public string GetItemidNew()
        {
            return string.Format(CultureInfo.CurrentCulture, "0x{0:X}", itemidNew);
        }
        /// <summary>
        ///  Formats lock type for display.  Parses bits and identifies them.
        /// </summary>
        /// <returns>Returns the formatted lock type.</returns>
        public string GetLockType()
        {
            string s = "";
            _VSRDTFLAGS mask = (_VSRDTFLAGS)dwRDTLockType;

            if ((mask & _VSRDTFLAGS.RDT_DontSave) != 0 &&
                (mask & _VSRDTFLAGS.RDT_DontSaveAs) != 0) s += "CantSave ";
            else
            {
                if ((mask & _VSRDTFLAGS.RDT_DontSave) != 0) s += "DontSave ";
                if ((mask & _VSRDTFLAGS.RDT_DontSaveAs) != 0) s += "DontSaveAs ";
            }
            if ((mask & _VSRDTFLAGS.RDT_ReadLock) != 0) s += "ReadLock ";
            if ((mask & _VSRDTFLAGS.RDT_EditLock) != 0) s += "EditLock ";
            
            if ((mask & _VSRDTFLAGS.RDT_RequestUnlock) != 0) s += "RequestUnlock ";
            if ((mask & _VSRDTFLAGS.RDT_NonCreatable) != 0) s += "NonCreatable ";
            if ((mask & _VSRDTFLAGS.RDT_DontAutoOpen) != 0) s += "DontAutoOpen ";
            if ((mask & _VSRDTFLAGS.RDT_CaseSensitive) != 0) s += "CaseSensitive ";

            if ((mask & _VSRDTFLAGS.RDT_Unlock_NoSave) != 0) s += "Unlock_NoSave ";
            if ((mask & _VSRDTFLAGS.RDT_Unlock_SaveIfDirty) != 0) s += "Unlock_SaveIfDirty ";
            if ((mask & _VSRDTFLAGS.RDT_Unlock_PromptSave) != 0) s += "Unlock_PromptSave ";

            if ((mask & _VSRDTFLAGS.RDT_VirtualDocument) != 0) s += "VirtualDocument ";
            if ((mask & _VSRDTFLAGS.RDT_ProjSlnDocument) != 0) s += "ProjSlnDocument ";
            if ((mask & _VSRDTFLAGS.RDT_PlaceHolderDoc) != 0) s += "PlaceHolderDoc ";
            if ((mask & _VSRDTFLAGS.RDT_CanBuildFromMemory) != 0) s += "CanBuildFromMemory ";
            if ((mask & _VSRDTFLAGS.RDT_DontAddToMRU) != 0) s += "DontAddToMRU ";

            return string.Format(CultureInfo.CurrentCulture, "0x{0:X} ", dwRDTLockType) + s;
        }
        /// <summary>
        ///  Formats attributes for display.  Parses bits and identifies them.
        /// </summary>
        /// <returns>Returns the formatted attributes.</returns>
        public string GetAttribs()
        {
            string s = "";
            __VSRDTATTRIB mask = (__VSRDTATTRIB) grfAttribs;
            if ((mask & __VSRDTATTRIB.RDTA_Hierarchy) != 0) s += "Hierarchy ";
            if ((mask & __VSRDTATTRIB.RDTA_ItemID) != 0) s += "ItemID ";
            if ((mask & __VSRDTATTRIB.RDTA_MkDocument) != 0) s += "FullPath ";
            if ((mask & __VSRDTATTRIB.RDTA_DocDataIsDirty) != 0) s += "DataIsDirty ";
            if ((mask & __VSRDTATTRIB.RDTA_DocDataIsNotDirty) != 0) s += "DataIsNotDirty ";
            if ((mask & __VSRDTATTRIB.RDTA_DocDataReloaded) != 0) s += "DocDataReloaded ";
            if ((mask & __VSRDTATTRIB.RDTA_AltHierarchyItemID) != 0) s += "AltHierarchyItemID ";

            return string.Format(CultureInfo.CurrentCulture, "0x{0:X} ", grfAttribs) + s;
        }
        #endregion
    }
    class AttributeEvent : GenericEvent
    {
        [DisplayName("Attributes")]
        [Description("The attribute flags.")]
        [Category("Extended")]
        public string Attribute { get { return GetAttribs(); } }

        public AttributeEvent(RunningDocumentTable rdt, string message, uint cookie, uint grfAttribs)
            : base(rdt, message, cookie)
        {
            this.grfAttribs = grfAttribs;
        }
    }
    class WindowFrameEvent : GenericEvent
    {
        public WindowFrameEvent(RunningDocumentTable rdt, string message, uint cookie, IVsWindowFrame pFrame)
            : base(rdt, message, cookie)
        {
            this.pFrame = pFrame;
        }
    }
    class LockEvent : GenericEvent
    {
        [DisplayName("Lock Type")]
        [Description("The lock flags.")]
        [Category("Extended")]
        public string LockType { get { return GetLockType(); } }

        public LockEvent(RunningDocumentTable rdt, string message, uint cookie, uint dwRDTLockType)
            : base(rdt, message, cookie)
        {
            this.dwRDTLockType = dwRDTLockType;
        }
    }
    class ShowEvent : GenericEvent
    {
        [DisplayName("First Show")]
        [Description("True if this is the first time the document is shown.")]
        [Category("Extended")]
        public int IsFirstShow { get { return fFirstShow; } }

        public ShowEvent(RunningDocumentTable rdt, string message, uint cookie, int fFirstShow, IVsWindowFrame pFrame)
            : base(rdt, message, cookie)
        {
            this.fFirstShow = fFirstShow;
            this.pFrame = pFrame;
        }
    }
    // Extended events.
    class EventEx : GenericEvent
    {
        [DisplayName("Hierarchy")]
        [Description("The caption of the old hierarchy root.")]
        [Category("Old")]
        public string OldHierarchyRoot
        {
            get 
            {
                if (OldHierarchy == null) return "null";
                object o = string.Empty;
                try
                {
                    OldHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int) __VSHPROPID.VSHPROPID_Caption, out o);
                }
                finally { }
                return (string) o;
            }
        }
        [DisplayName("Document")]
        [Description("The long name of the old document.")]
        [Category("Old")]
        public string OldDocMoniker
        {
            get { return pszMkDocumentOld; }
        }
        [DisplayName("Item ID")]
        [Description("The old item ID.")]
        [Category("Old")]
        public string OldItemId { get { return GetItemidOld(); } }

        public EventEx(RunningDocumentTable rdt, string message, uint cookie, 
            IVsHierarchy oldHierarchy, uint itemidOld, string pszMkDocumentOld)
            : base(rdt, message, cookie)
        {
            OldHierarchy = oldHierarchy;
            this.itemidOld = itemidOld;
            this.pszMkDocumentOld = pszMkDocumentOld;
        }
    }
    class AttributeEventEx : EventEx
    {
        [DisplayName("Attributes")]
        [Description("The attribute flags.")]
        [Category("Extended")]
        public string Attribute { get { return GetAttribs(); } }

        [DisplayName("Hierarchy")]
        [Description("The caption of the new hierarchy root.")]
        [Category("New")]
        public string NewHierarchyRoot
        {
            get
            {
                if (NewHierarchy == null) return "null";
                object o = string.Empty;
                try
                {
                    NewHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Caption, out o);
                }
                finally { }
                return (string)o;
            }
        }
        [DisplayName("Document")]
        [Description("The long name of the new document.")]
        [Category("New")]
        public string NewDocMoniker
        {
            get { return pszMkDocumentNew; }
        }
        [DisplayName("Item ID")]
        [Description("The new item ID.")]
        [Category("New")]
        public string NewItemId { get { return GetItemidNew(); } }

        public AttributeEventEx(RunningDocumentTable rdt, string message, uint cookie, uint grfAttribs,
            IVsHierarchy oldHierarchy, uint itemidOld, string pszMkDocumentOld, 
            IVsHierarchy newHierarchy, uint itemidNew, string pszMkDocumentNew)
            : base(rdt, message, cookie, oldHierarchy, itemidOld, pszMkDocumentOld)
        {
            this.grfAttribs = grfAttribs;
            NewHierarchy = newHierarchy;
            this.itemidNew = itemidNew;
            this.pszMkDocumentNew = pszMkDocumentNew;
        }
    }
    class UnlockEventEx : EventEx
    {
        [DisplayName("Close, No Save")]
        [Description("True if the document is closed without saving.")]
        [Category("Extended")]
        public int IsCloseNoSave { get { return fClosedWithoutSaving; } }

        public UnlockEventEx(string message,
            IVsHierarchy pHier, uint itemid, string pszMkDocument, int fClosedWithoutSaving)
            : base(null, message, 0, pHier, itemid, pszMkDocument)
        {
            this.fClosedWithoutSaving = fClosedWithoutSaving;
        }
    }
    class LockEventEx : EventEx
    {
        public LockEventEx(string message,
            IVsHierarchy pHier, uint itemid, string pszMkDocument)
            : base(null, message, 0, pHier, itemid, pszMkDocument)
        {
            OldHierarchy = pHier;
            itemidOld = itemid;
            pszMkDocumentOld = pszMkDocument;
        }
    }
}
