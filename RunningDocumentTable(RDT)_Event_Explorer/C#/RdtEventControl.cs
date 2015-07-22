/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Globalization;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MsVsShell = Microsoft.VisualStudio.Shell;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using EnvDTE;

namespace MyCompany.RdtEventExplorer
{
    /// <summary>
    /// The RDT event explorer user control.
    /// Displays a log of events in a data grid.
    /// When an event is selected, its details appear in the Properties window.
    /// </summary>
    public partial class RdtEventControl : UserControl, IDisposable, 
        IVsRunningDocTableEvents, IVsRunningDocTableEvents2, IVsRunningDocTableEvents3, IVsRunningDocTableEvents4
    {
        // RDT
        uint rdtCookie;
        RunningDocumentTable rdt;

        // Selection container
        MsVsShell.SelectionContainer selectionContainer;

        // A reference to the single copy of the options.
        Options options;

        #region Constructor
        /// <summary>
        /// The event explorer user control constructor.
        /// </summary>
        public RdtEventControl()
        {
            InitializeComponent();

            // Create a selection container for tracking selected RDT events.
            selectionContainer = new MsVsShell.SelectionContainer();

            // Advise the RDT of this event sink.
            IOleServiceProvider sp = 
                Package.GetGlobalService(typeof(IOleServiceProvider)) as IOleServiceProvider;
            if (sp == null) return;

            rdt = new RunningDocumentTable(new ServiceProvider(sp));
            if (rdt == null) return;

            rdtCookie = rdt.Advise(this);

            // Obtain the single instance of the options via automation. 
            try
            {
                DTE dte = (DTE)Package.GetGlobalService(typeof(DTE));

                Properties props =
                   dte.get_Properties("RDT Event Explorer", "Explorer Options");

                IOptions o = props.Item("ContainedOptions").Object as IOptions;
                options = (Options)o;
            }
            catch
            {
                IVsActivityLog log = Package.GetGlobalService(
                    typeof(SVsActivityLog)) as IVsActivityLog;
                if (log != null)
                {

                    log.LogEntry(
                        (uint)__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION,
                        ToString(),
                        string.Format(CultureInfo.CurrentCulture,
                            "RdtEventExplorer could not obtain properties via automation: {0}", 
                            ToString())
                    );
                }
                options = new Options();
            }
            // Prepare the event grid.
            eventGrid.AutoGenerateColumns = false;
            eventGrid.AllowUserToAddRows = false;
            eventGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            eventGrid.Columns.Add("Event", Resources.EventHeader);
            eventGrid.Columns.Add("Moniker", Resources.MonikerHeader);
            eventGrid.Columns["Event"].ReadOnly = true;
            eventGrid.Columns["Moniker"].ReadOnly = true;

            eventGrid.AllowUserToResizeRows = false;
            eventGrid.AllowUserToResizeColumns = true;
            eventGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            
            int x = Screen.PrimaryScreen.Bounds.Size.Width;
            int y = Screen.PrimaryScreen.Bounds.Size.Height;
            Size = new Size(x / 3, y / 3);
        }
        #endregion
        #region IDisposable Members
        void IDisposable.Dispose()
        {
            try 
            {
                if (rdtCookie != 0) rdt.Unadvise(rdtCookie);
            }
            finally 
            { 
                Dispose(); 
            }
        }
        #endregion
        #region ProcessDialogChar
        /// <summary> 
        /// Let this control process the mnemonics.
        /// </summary>
        protected override bool ProcessDialogChar(char charCode)
        {
            // If we're the top-level form or control, we need to do the mnemonic handling
            if (charCode != ' ' && ProcessMnemonic(charCode))
            {
                return true;
            }
            return base.ProcessDialogChar(charCode);
        }
        #endregion

        #region Add Event to Grid
        /// <summary>
        /// Adds an RDT event wrapper to the grid.
        /// </summary>
        /// <param name="ev"></param>
        public void AddEventToGrid(GenericEvent ev)
        {
            if (ev == null) return;

            int n = eventGrid.Rows.Add();
            DataGridViewRow row = eventGrid.Rows[n];
            row.Cells["Event"].Value = ev.EventName;
            row.Cells["Moniker"].Value = ev.DocumentName;
            row.Tag = ev;
        }
        #endregion
        #region IVsRunningDocTableEvents Members
        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            if (options.OptAfterAttributeChange)
            {
                AddEventToGrid(new AttributeEvent(rdt, "OnAfterAttributeChange", docCookie, grfAttribs));
            }
            return VSConstants.S_OK;
        }
        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            if (options.OptAfterDocumentWindowHide)
            {
                AddEventToGrid(new WindowFrameEvent(rdt, "OnAfterDocumentWindowHide", docCookie, pFrame));
            }
            return VSConstants.S_OK;
        }
        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            if (options.OptAfterFirstDocumentLock)
            {
                AddEventToGrid(new LockEvent(rdt, "OnAfterFirstDocumentLock", docCookie, dwRDTLockType));
            }
            return VSConstants.S_OK;
        }
        public int OnAfterSave(uint docCookie)
        {   
            if (options.OptAfterSave)
            {
                AddEventToGrid(new GenericEvent(rdt, "OnAfterSave", docCookie));
            }
            return VSConstants.S_OK;
        }
        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            if (options.OptBeforeDocumentWindowShow)
            {
                AddEventToGrid(new ShowEvent(rdt, "OnBeforeDocumentWindowShow", docCookie, fFirstShow, pFrame));
            }
            return VSConstants.S_OK;
        }
        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            if (options.OptBeforeLastDocumentUnlock)
            {
                AddEventToGrid(new LockEvent(rdt, "OnBeforeLastDocumentUnlock", docCookie, dwRDTLockType));
            }
            return VSConstants.S_OK;
        }
        #endregion
        #region IVsRunningDocTableEvents2 Members
        public int OnAfterAttributeChangeEx(
            uint docCookie, uint grfAttribs, 
            IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, 
            IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            if (options.OptAfterAttributeChangeEx)
            {
                AddEventToGrid(new AttributeEventEx(rdt, "OnAfterAttributeChangeEx", docCookie, grfAttribs, 
                    pHierOld, itemidOld, pszMkDocumentOld,
                    pHierNew, itemidNew, pszMkDocumentNew));
            }
            return VSConstants.S_OK;
        }
        #endregion
        #region IVsRunningDocTableEvents3 Members
        public int OnBeforeSave(uint docCookie)
        {
            if (options.OptBeforeSave)
            {
                AddEventToGrid(new GenericEvent(rdt, "OnBeforeSave", docCookie));
            }
            return VSConstants.S_OK;
        }
        #endregion
        #region IVsRunningDocTableEvents4 Members
        public int OnAfterLastDocumentUnlock(IVsHierarchy pHier, uint itemid, string pszMkDocument, int fClosedWithoutSaving)
        {
            if (options.OptAfterLastDocumentUnlock)
            {
                AddEventToGrid(new UnlockEventEx("OnAfterLastDocumentUnlock",  
                    pHier, itemid, pszMkDocument, fClosedWithoutSaving));
            }
            return VSConstants.S_OK;
        }
        public int OnAfterSaveAll()
        {
            if (options.OptAfterSaveAll)
            {
                AddEventToGrid(new GenericEvent(null, "OnAfterSaveAll", 0));
            }
            return VSConstants.S_OK;
        }
        public int OnBeforeFirstDocumentLock(IVsHierarchy pHier, uint itemid, string pszMkDocument)
        {
            if (options.OptBeforeFirstDocumentLock)
            {
                AddEventToGrid(new LockEventEx("OnBeforeFirstDocumentLock",
                    pHier, itemid, pszMkDocument));
            }
            return VSConstants.S_OK;
        }
        #endregion

        #region Selection tracking
        // Cached Selection tracking service used to expose properties
        ITrackSelection trackSelection;
        
        /// <summary>
        /// Track selection service for the tool window.
        /// This should be set by the tool window pane as soon as the tool
        /// window is created.
        /// </summary>
        internal ITrackSelection TrackSelection
        {
            get
            {
                return trackSelection;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("TrackSelection");
                trackSelection = value;
                // Inititalize with an empty selection
                // Failure to do this would result in our later calls to 
                // OnSelectChange to be ignored (unless focus is lost
                // and regained).
                selectionContainer.SelectableObjects = null;
                selectionContainer.SelectedObjects = null;
                trackSelection.OnSelectChange(selectionContainer);
            }
        }
        /// <summary>
        // Update the selection in the Properties window.
        /// </summary>
        public void UpdateSelection()
        {
            ITrackSelection track = TrackSelection;
            if (track != null)
                track.OnSelectChange((ISelectionContainer)selectionContainer);
        }
        /// <summary>
        // Update the selection container.
        /// </summary>
        /// <param name="list">list of objects to be selected and selectable</param>
        public void SelectList(ArrayList list)
        {
            selectionContainer = new MsVsShell.SelectionContainer(true, false);
            selectionContainer.SelectableObjects = list;
            selectionContainer.SelectedObjects = list;
            UpdateSelection();
        }
        #endregion        
        #region Control events
        /// <summary>
        /// Clear event lines from grid and refresh display to show empty grid.
        /// </summary>
        public void ClearGrid()
        {
            eventGrid.Rows.Clear();
            eventGrid.Refresh();
        }
        /// <summary>
        /// Refresh the grid display.  May not be needed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RefreshGrid()
        {
            eventGrid.Refresh();
        }
        /// <summary>
        /// Track the event associated with selected grid row in the Properties window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void eventGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore click on header row.
            if (e.RowIndex < 0) return;

            // Find the selected row.
            DataGridViewRow row = eventGrid.Rows[e.RowIndex];
            // Recover the associated event object.
            GenericEvent ev = (GenericEvent)row.Tag;

            // Create an array of one event object and track it in the Properties window.
            ArrayList listObjects = new ArrayList();
            listObjects.Add(ev);
            SelectList(listObjects);
        }
        #endregion
    }
}
