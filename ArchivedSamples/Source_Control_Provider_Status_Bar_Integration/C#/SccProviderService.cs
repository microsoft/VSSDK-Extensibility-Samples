/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

// SccProviderService.cs : Implementation of Sample Source Control Provider Service
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.SccProvider
{
    [Guid("B0BAC05D-1000-41D1-A6C3-704E6C1A3DE2")]
    public partial class SccProviderService : 
        IVsSccProvider,             // Required for provider registration with source control manager
        IVsSccManager2,             // Base source control functionality interface
        IVsSccManagerTooltip,       // Provide tooltips for source control items
        IVsSolutionEvents,          // We'll register for solution events, these are usefull for source control
        IVsSolutionEvents2,
        IVsQueryEditQuerySave2,     // Required to allow editing of controlled files 
        IVsTrackProjectDocumentsEvents2,  // Usefull to track project changes (add, renames, deletes, etc)
        IVsSccSolution,
        IDisposable 
    {
        // Whether the provider is active or not
        private bool _active = false;
        // The service and source control provider
        private SccProvider _sccProvider = null;
        // The cookie for solution events 
        private uint _vsSolutionEventsCookie;
        // The cookie for project document events
        private uint _tpdTrackProjectDocumentsCookie;
        // The list of controlled projects hierarchies
        private Hashtable _controlledProjects = new Hashtable();
        // The list of controlled and offline projects hierarchies
        private Hashtable _offlineProjects = new Hashtable();
        // Variable tracking whether the currently loading solution is controlled (during solution load or merge)
        private string _loadingControlledSolutionLocation = "";
        // The location of the currently controlled solution
        private string _solutionLocation;
        // The list of files approved for in-memory edit
        private Hashtable _approvedForInMemoryEdit = new Hashtable();

        #region SccProvider Service initialization/unitialization

        public SccProviderService(SccProvider sccProvider)
        {
            Debug.Assert(null != sccProvider);
            _sccProvider = sccProvider;

            // Subscribe to solution events
            IVsSolution sol = (IVsSolution)_sccProvider.GetService(typeof(SVsSolution));
            sol.AdviseSolutionEvents(this, out _vsSolutionEventsCookie);
            Debug.Assert(VSConstants.VSCOOKIE_NIL != _vsSolutionEventsCookie);

            // Subscribe to project documents
            IVsTrackProjectDocuments2 tpdService = (IVsTrackProjectDocuments2)_sccProvider.GetService(typeof(SVsTrackProjectDocuments));
            tpdService.AdviseTrackProjectDocumentsEvents(this, out _tpdTrackProjectDocumentsCookie);
            Debug.Assert(VSConstants.VSCOOKIE_NIL != _tpdTrackProjectDocumentsCookie);
        }

        public void Dispose()
        {
            // Unregister from receiving solution events
            if (VSConstants.VSCOOKIE_NIL != _vsSolutionEventsCookie)
            {
                IVsSolution sol = (IVsSolution)_sccProvider.GetService(typeof(SVsSolution));
                sol.UnadviseSolutionEvents(_vsSolutionEventsCookie);
                _vsSolutionEventsCookie = VSConstants.VSCOOKIE_NIL;
            }

            // Unregister from receiving project documents
            if (VSConstants.VSCOOKIE_NIL != _tpdTrackProjectDocumentsCookie)
            {
                IVsTrackProjectDocuments2 tpdService = (IVsTrackProjectDocuments2)_sccProvider.GetService(typeof(SVsTrackProjectDocuments));
                tpdService.UnadviseTrackProjectDocumentsEvents(_tpdTrackProjectDocumentsCookie);
                _tpdTrackProjectDocumentsCookie = VSConstants.VSCOOKIE_NIL;
            }
        }

        #endregion

        //--------------------------------------------------------------------------------
        // IVsSccProvider specific functions
        //--------------------------------------------------------------------------------
        #region IVsSccProvider interface functions

        // Called by the scc manager when the provider is activated. 
        // Make visible and enable if necessary scc related menu commands
        public int SetActive()
        {
            Debug.WriteLine(String.Format(CultureInfo.CurrentUICulture, "The source control provider is now active"));

            _active = true;
            _sccProvider.OnActiveStateChange();

            return VSConstants.S_OK;
        }

        // Called by the scc manager when the provider is deactivated. 
        // Hides and disable scc related menu commands
        public int SetInactive()
        {
            Debug.WriteLine(String.Format(CultureInfo.CurrentUICulture, "The source control provider is now inactive"));

            _active = false;
            _sccProvider.OnActiveStateChange();

            return VSConstants.S_OK;
        }

        // Called by the scc manager when the user wants to switch to a different source control provider
        // to see if the user needs to be prompted for closing the current solution (should anything be 
        // under the control of this provider)
        public int AnyItemsUnderSourceControl(out int pfResult)
        {
            if (!_active)
            {
                pfResult = 0;
            }
            else
            {
                // Although the parameter is an int, it's in reality a BOOL value, so let's return 0/1 values
                pfResult = (_controlledProjects.Count != 0) ? 1 : 0;
            }
    
            return VSConstants.S_OK;
        }

        #endregion

        //--------------------------------------------------------------------------------
        // IVsSccManager2 specific functions
        //--------------------------------------------------------------------------------
        #region IVsSccManager2 interface functions

        public int BrowseForProject(out string pbstrDirectory, out int pfOK)
        {
            // Obsolete method
            pbstrDirectory = null;
            pfOK = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int CancelAfterBrowseForProject() 
        {
            // Obsolete method
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Returns whether the source control provider is fully installed
        /// </summary>
        public int IsInstalled(out int pbInstalled)
        {
            // All source control packages should always return S_OK and set pbInstalled to nonzero
            pbInstalled = 1;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Provide source control icons for the specified files and returns scc status of files
        /// </summary>
        /// <returns>The method returns S_OK if at least one of the files is controlled, S_FALSE if none of them are</returns>
        public int GetSccGlyph( [InAttribute] int cFiles, [InAttribute] string[] rgpszFullPaths, [OutAttribute] VsStateIcon[] rgsiGlyphs, [OutAttribute] uint[] rgdwSccStatus )
        {
            Debug.Assert(cFiles == 1, "Only getting one file icon at a time is supported");

            // Return the icons and the status. While the status is a combination a flags, we'll return just values 
            // with one bit set, to make life easier for GetSccGlyphsFromStatus
            SourceControlStatus status = GetFileStatus(rgpszFullPaths[0]);
            switch (status)
            {
                case SourceControlStatus.scsCheckedIn:
                    rgsiGlyphs[0] = VsStateIcon.STATEICON_CHECKEDIN;
                    if (rgdwSccStatus != null)
                    {
                        rgdwSccStatus[0] = (uint) __SccStatus.SCC_STATUS_CONTROLLED;
                    }
                    break;
                case SourceControlStatus.scsCheckedOut:
                    rgsiGlyphs[0] = VsStateIcon.STATEICON_CHECKEDOUT;
                    if (rgdwSccStatus != null)
                    {
                        rgdwSccStatus[0] = (uint) __SccStatus.SCC_STATUS_CHECKEDOUT;
                    }
                    break;
                default:
                    IList<VSITEMSELECTION> nodes = GetControlledProjectsContainingFile(rgpszFullPaths[0]);
                    if (nodes.Count > 0)
                    {
                        // If the file is not controlled, but is member of a controlled project, report the item as checked out (same as source control in VS2003 did)
                        // If the provider wants to have special icons for "pending add" files, the IVsSccGlyphs interface needs to be supported
                        rgsiGlyphs[0] = VsStateIcon.STATEICON_CHECKEDOUT;
                        if (rgdwSccStatus != null)
                        {
                            rgdwSccStatus[0] = (uint) __SccStatus.SCC_STATUS_CHECKEDOUT;
                        }
                    }
                    else
                    {
                        // This is an uncontrolled file, return a blank scc glyph for it
                        rgsiGlyphs[0] = VsStateIcon.STATEICON_BLANK;
                        if (rgdwSccStatus != null)
                        {
                            rgdwSccStatus[0] = (uint) __SccStatus.SCC_STATUS_NOTCONTROLLED;
                        }
                    }
                    break;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines the corresponding scc status glyph to display, given a combination of scc status flags
        /// </summary>
        public int GetSccGlyphFromStatus([InAttribute] uint dwSccStatus, [OutAttribute] VsStateIcon[] psiGlyph)
        {
            switch (dwSccStatus)
            {
                case (uint) __SccStatus.SCC_STATUS_CHECKEDOUT:
                    psiGlyph[0] = VsStateIcon.STATEICON_CHECKEDOUT;
                    break;
                case (uint) __SccStatus.SCC_STATUS_CONTROLLED:
                    psiGlyph[0] = VsStateIcon.STATEICON_CHECKEDIN;
                    break;
                default:
                    psiGlyph[0] = VsStateIcon.STATEICON_BLANK;
                    break;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// One of the most important methods in a source control provider, is called by projects that are under source control when they are first opened to register project settings
        /// </summary>
        public int RegisterSccProject([InAttribute] IVsSccProject2 pscp2Project, [InAttribute] string pszSccProjectName, [InAttribute] string pszSccAuxPath, [InAttribute] string pszSccLocalPath, [InAttribute] string pszProvider)
        {
            if (pszProvider.CompareTo(_sccProvider.ProviderName)!=0)
            {
                // If the provider name controlling this project is not our provider, the user may be adding to a 
                // solution controlled by this provider an existing project controlled by some other provider.
                // We'll deny the registration with scc in such case.
                return VSConstants.E_FAIL;
            }

            if (pscp2Project == null)
            {
                // Manual registration with source control of the solution, from OnAfterOpenSolution
                Debug.WriteLine(String.Format(CultureInfo.CurrentUICulture, "Solution {0} is registering with source control - {1}, {2}, {3}, {4}", _sccProvider.GetSolutionFileName(), pszSccProjectName, pszSccAuxPath, pszSccLocalPath, pszProvider));

                IVsHierarchy solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
                string solutionFile = _sccProvider.GetSolutionFileName();
                SccProviderStorage storage = new SccProviderStorage(solutionFile);
                _controlledProjects[solHier] = storage;
            }
            else
            {
                Debug.WriteLine(String.Format(CultureInfo.CurrentUICulture, "Project {0} is registering with source control - {1}, {2}, {3}, {4}", _sccProvider.GetProjectFileName(pscp2Project), pszSccProjectName, pszSccAuxPath, pszSccLocalPath, pszProvider));

                // Add the project to the list of controlled projects
                IVsHierarchy hierProject = (IVsHierarchy)pscp2Project;
                SccProviderStorage storage = new SccProviderStorage(_sccProvider.GetProjectFileName(pscp2Project));
                _controlledProjects[hierProject] = storage;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by projects registered with the source control portion of the environment before they are closed. 
        /// </summary>
        public int UnregisterSccProject([InAttribute] IVsSccProject2 pscp2Project)
        {
            // Get the project's hierarchy
            IVsHierarchy hierProject = null;
            if (pscp2Project == null)
            {
                // If the project's pointer is null, it must be the solution calling to unregister, from OnBeforeCloseSolution
                Debug.WriteLine(String.Format(CultureInfo.CurrentUICulture, "Solution {0} is unregistering with source control.", _sccProvider.GetSolutionFileName()));
                hierProject = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
            }
            else
            {
                Debug.WriteLine(String.Format(CultureInfo.CurrentUICulture, "Project {0} is unregistering with source control.", _sccProvider.GetProjectFileName(pscp2Project)));
                hierProject = (IVsHierarchy)pscp2Project;
            }

            // Remove the project from the list of controlled projects
            if (_controlledProjects.ContainsKey(hierProject))
            {
                _controlledProjects.Remove(hierProject);
                return VSConstants.S_OK;
            }
            else
            {
                return VSConstants.S_FALSE;
            }
        }

        #endregion

        //--------------------------------------------------------------------------------
        // IVsSccManagerTooltip specific functions
        //--------------------------------------------------------------------------------
        #region IVsSccManagerTooltip interface functions

        /// <summary>
        /// Called by solution explorer to provide tooltips for items. Returns a text describing the source control status of the item.
        /// </summary>
        public int GetGlyphTipText([InAttribute] IVsHierarchy phierHierarchy, [InAttribute] uint itemidNode, out string pbstrTooltipText)
        {
            // Initialize output parameters
            pbstrTooltipText = "";

            IList<string> files = _sccProvider.GetNodeFiles(phierHierarchy, itemidNode);
            if (files.Count == 0)
            {
                return VSConstants.S_OK;
            }

            // Return the glyph text based on the first file of node (the master file)
            SourceControlStatus status = GetFileStatus(files[0]);
            switch (status)
            {
                case SourceControlStatus.scsCheckedIn:
                    pbstrTooltipText = Resources.ResourceManager.GetString("Status_CheckedIn"); 
                    break;
                case SourceControlStatus.scsCheckedOut:
                    pbstrTooltipText = Resources.ResourceManager.GetString("Status_CheckedOut");
                    break;
                default:
                    // If the file is not controlled, but is member of a controlled project, report the item as checked out (same as source control in VS2003 did)
                    // If the provider wants to have special icons for "pending add" files, the IVsSccGlyphs interface needs to be supported
                    IList<VSITEMSELECTION> nodes = GetControlledProjectsContainingFile(files[0]);
                    if (nodes.Count > 0)
                    {
                        pbstrTooltipText = Resources.ResourceManager.GetString("Status_PendingAdd");
                    }
                    break;
            }

            return VSConstants.S_OK;
        }

        #endregion

        //--------------------------------------------------------------------------------
        // IVsSolutionEvents and IVsSolutionEvents2 specific functions
        //--------------------------------------------------------------------------------
        #region IVsSolutionEvents interface functions

        public int OnAfterCloseSolution([InAttribute] Object pUnkReserved)
        {
            // Reset all source-control-related data now that solution is closed
            _controlledProjects.Clear();
            _offlineProjects.Clear();
            _sccProvider.SolutionHasDirtyProps = false;
            _loadingControlledSolutionLocation = "";
            _solutionLocation = "";
            _approvedForInMemoryEdit.Clear();

            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject([InAttribute] IVsHierarchy pStubHierarchy, [InAttribute] IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject([InAttribute] IVsHierarchy pHierarchy, [InAttribute] int fAdded)
        {
            // If a solution folder is added to the solution after the solution is added to scc, we need to controll that folder
            if (_sccProvider.IsSolutionFolderProject(pHierarchy) && (fAdded == 1))
            {
                IVsHierarchy solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
                if (IsProjectControlled(solHier))
                {
                    // Register this solution folder using the same location as the solution
                    IVsSccProject2 pSccProject = (IVsSccProject2)pHierarchy;
                    RegisterSccProject(pSccProject, _solutionLocation, "", "", _sccProvider.ProviderName);

                    // We'll also need to refresh the solution folders glyphs to reflect the controlled state
                    IList<VSITEMSELECTION> nodes = new List<VSITEMSELECTION>();

                    VSITEMSELECTION vsItem;
                    vsItem.itemid = VSConstants.VSITEMID_ROOT;
                    vsItem.pHier = pHierarchy;
                    nodes.Add(vsItem);

                    _sccProvider.RefreshNodesGlyphs(nodes);
                }
            }

            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution([InAttribute] Object pUnkReserved, [InAttribute] int fNewSolution)
        {
            // This event is fired last by the shell when opening a solution.
            // By this time, we have already loaded the solution persistence data from the PreLoad section
            // the controlled projects should be opened and registered with source control
            if (_loadingControlledSolutionLocation.Length > 0)
            {
                // We'll also need to refresh the solution glyphs to reflect the controlled state
                IList<VSITEMSELECTION> nodes = new List<VSITEMSELECTION>();

                // If the solution was controlled, now it is time to register the solution hierarchy with souce control, too.
                // Note that solution is not calling RegisterSccProject(), the scc package will do this call as it knows the source control location
                RegisterSccProject(null, _loadingControlledSolutionLocation, "", "", _sccProvider.ProviderName);

                VSITEMSELECTION vsItem;
                vsItem.itemid = VSConstants.VSITEMID_ROOT;
                vsItem.pHier = null;
                nodes.Add(vsItem);

                // Also, solution folders won't call RegisterSccProject, so we have to enumerate them and register them with scc once the solution is controlled
                Hashtable enumSolFolders = _sccProvider.GetSolutionFoldersEnum();
                foreach (IVsHierarchy pHier in enumSolFolders.Keys)
                {
                    // Register this solution folder using the same location as the solution
                    IVsSccProject2 pSccProject = (IVsSccProject2)pHier;
                    RegisterSccProject(pSccProject, _loadingControlledSolutionLocation, "", "", _sccProvider.ProviderName);

                    vsItem.itemid = VSConstants.VSITEMID_ROOT;
                    vsItem.pHier = pHier;
                    nodes.Add(vsItem);
                }

                // Refresh the glyphs now for solution and solution folders
                _sccProvider.RefreshNodesGlyphs(nodes);
            }

            _solutionLocation = _loadingControlledSolutionLocation;

            // reset the flag now that solution open completed
            _loadingControlledSolutionLocation = "";

            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject([InAttribute] IVsHierarchy pHierarchy, [InAttribute] int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution([InAttribute] Object pUnkReserved)
        {
            // Since we registered the solution with source control from OnAfterOpenSolution, it would be nice to unregister it, too, when it gets closed.
            // Also, unregister the solution folders
            Hashtable enumSolFolders = _sccProvider.GetSolutionFoldersEnum();
            foreach (IVsHierarchy pHier in enumSolFolders.Keys)
            {
                IVsSccProject2 pSccProject = (IVsSccProject2)pHier;
                UnregisterSccProject(pSccProject);
            }

            UnregisterSccProject(null);

            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject([InAttribute] IVsHierarchy pRealHierarchy, [InAttribute] IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject([InAttribute] IVsHierarchy pHierarchy, [InAttribute] int fRemoving, [InAttribute] ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution([InAttribute] Object pUnkReserved, [InAttribute] ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject([InAttribute] IVsHierarchy pRealHierarchy, [InAttribute] ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterMergeSolution ([InAttribute] Object pUnkReserved )
        {
            // reset the flag now that solutions were merged and the merged solution completed opening
            _loadingControlledSolutionLocation = "";

            return VSConstants.S_OK;
        }

        #endregion

        //--------------------------------------------------------------------------------
        // IVsQueryEditQuerySave2 specific functions
        //--------------------------------------------------------------------------------
        #region IVsQueryEditQuerySave2 interface functions

        public int BeginQuerySaveBatch ()
        {
            return VSConstants.S_OK;
        }

        public int EndQuerySaveBatch ()
        {
            return VSConstants.S_OK;
        }

        public int DeclareReloadableFile([InAttribute] string pszMkDocument, [InAttribute] uint rgf, [InAttribute] VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        public int DeclareUnreloadableFile([InAttribute] string pszMkDocument, [InAttribute] uint rgf, [InAttribute] VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        public int IsReloadable ([InAttribute] string pszMkDocument, out int pbResult )
        {
            // Since we're not tracking which files are reloadable and which not, consider everything reloadable
            pbResult = 1;
            return VSConstants.S_OK;
        }

        public int OnAfterSaveUnreloadableFile([InAttribute] string pszMkDocument, [InAttribute] uint rgf, [InAttribute] VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by projects and editors before modifying a file
        /// The function allows the source control systems to take the necessary actions (checkout, flip attributes)
        /// to make the file writable in order to allow the edit to continue
        ///
        /// There are a lot of cases to deal with during QueryEdit/QuerySave. 
        /// - called in commmand line mode, when UI cannot be displayed
        /// - called during builds, when save shoudn't probably be allowed
        /// - called during projects migration, when projects are not open and not registered yet with source control
        /// - checking out files may bring new versions from vss database which may be reloaded and the user may lose in-memory changes; some other files may not be reloadable
        /// - not all editors call QueryEdit when they modify the file the first time (buggy editors!), and the files may be already dirty in memory when QueryEdit is called
        /// - files on disk may be modified outside IDE and may have attributes incorrect for their scc status
        /// - checkouts may fail
        /// The sample provider won't deal with all these situations, but a real source control provider should!
        /// </summary>
        public int QueryEditFiles([InAttribute] uint rgfQueryEdit, [InAttribute] int cFiles, [InAttribute] string[] rgpszMkDocuments, [InAttribute] uint[] rgrgf, [InAttribute] VSQEQS_FILE_ATTRIBUTE_DATA[] rgFileInfo, out uint pfEditVerdict, out uint prgfMoreInfo)
        {
            // Initialize output variables
            pfEditVerdict = (uint)tagVSQueryEditResult.QER_EditOK;
            prgfMoreInfo = 0;

            // In non-UI mode just allow the edit, because the user cannot be asked what to do with the file
            if (_sccProvider.InCommandLineMode())
            {
                return VSConstants.S_OK;
            }

            try 
            {
                //Iterate through all the files
                for (int iFile = 0; iFile < cFiles; iFile++)
                {
                     
                    uint fEditVerdict = (uint)tagVSQueryEditResult.QER_EditNotOK;
                    uint fMoreInfo = 0;

                    // Because of the way we calculate the status, it is not possible to have a 
                    // checked in file that is writtable on disk, or a checked out file that is read-only on disk
                    // A source control provider would need to deal with those situations, too
                    SourceControlStatus status = GetFileStatus(rgpszMkDocuments[iFile]);
                    bool fileExists = File.Exists(rgpszMkDocuments[iFile]);
                    bool isFileReadOnly = false;
                    if (fileExists)
                    {
                        isFileReadOnly = (( File.GetAttributes(rgpszMkDocuments[iFile]) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
                    }

                    // Allow the edits if the file does not exist or is writable
                    if (!fileExists || !isFileReadOnly)
                    {
                        fEditVerdict = (uint)tagVSQueryEditResult.QER_EditOK;
                    }
                    else
                    {
                        // If the IDE asks about a file that was already approved for in-memory edit, allow the edit without asking the user again
                        if (_approvedForInMemoryEdit.ContainsKey(rgpszMkDocuments[iFile].ToLower()))
                        {
                            fEditVerdict = (uint)tagVSQueryEditResult.QER_EditOK;
                            fMoreInfo = (uint)(tagVSQueryEditResultFlags.QER_InMemoryEdit);
                        }
                        else
                        {
                            switch (status)
                            {
                                case SourceControlStatus.scsCheckedIn:
                                    if ((rgfQueryEdit & (uint)tagVSQueryEditFlags.QEF_ReportOnly) != 0)
                                    {
                                        fMoreInfo = (uint)(tagVSQueryEditResultFlags.QER_EditNotPossible | tagVSQueryEditResultFlags.QER_ReadOnlyUnderScc);
                                    }
                                    else
                                    {
                                        DlgQueryEditCheckedInFile dlgAskCheckout = new DlgQueryEditCheckedInFile(rgpszMkDocuments[iFile]);
                                        if ((rgfQueryEdit & (uint)tagVSQueryEditFlags.QEF_SilentMode) != 0)
                                        {
                                            // When called in silent mode, attempt the checkout
                                            // (The alternative is to deny the edit and return QER_NoisyPromptRequired and expect for a non-silent call)
                                            dlgAskCheckout.Answer = DlgQueryEditCheckedInFile.qecifCheckout;
                                        }
                                        else
                                        {
                                            dlgAskCheckout.ShowDialog();
                                        }

                                        if (dlgAskCheckout.Answer == DlgQueryEditCheckedInFile.qecifCheckout)
                                        {
                                            // Increase the pending change count, since the user checked out a file
                                            PendingChangeCount++;

                                            // Checkout the file, and since it cannot fail, allow the edit
                                            CheckoutFileAndRefreshProjectGlyphs(rgpszMkDocuments[iFile]);
                                            fEditVerdict = (uint)tagVSQueryEditResult.QER_EditOK;
                                            fMoreInfo = (uint)tagVSQueryEditResultFlags.QER_MaybeCheckedout;
                                            // Do not forget to set QER_Changed if the content of the file on disk changes during the query edit
                                            // Do not forget to set QER_Reloaded if the source control reloads the file from disk after such changing checkout.
                                        }
                                        else if (dlgAskCheckout.Answer == DlgQueryEditCheckedInFile.qecifEditInMemory)
                                        {
                                            // Allow edit in memory
                                            fEditVerdict = (uint)tagVSQueryEditResult.QER_EditOK;
                                            fMoreInfo = (uint)(tagVSQueryEditResultFlags.QER_InMemoryEdit);
                                            // Add the file to the list of files approved for edit, so if the IDE asks again about this file, we'll allow the edit without asking the user again
                                            // UNDONE: Currently, a file gets removed from _approvedForInMemoryEdit list only when the solution is closed. Consider intercepting the 
                                            // IVsRunningDocTableEvents.OnAfterSave/OnAfterSaveAll interface and removing the file from the approved list after it gets saved once.
                                            _approvedForInMemoryEdit[rgpszMkDocuments[iFile].ToLower()] = true;
                                        }
                                        else
                                        {
                                            fEditVerdict = (uint)tagVSQueryEditResult.QER_NoEdit_UserCanceled;
                                            fMoreInfo = (uint)(tagVSQueryEditResultFlags.QER_ReadOnlyUnderScc | tagVSQueryEditResultFlags.QER_CheckoutCanceledOrFailed);
                                        }

                                    }
                                    break;
                                case SourceControlStatus.scsCheckedOut: // fall through
                                case SourceControlStatus.scsUncontrolled:
                                    if (fileExists && isFileReadOnly)
                                    {
                                        if ((rgfQueryEdit & (uint)tagVSQueryEditFlags.QEF_ReportOnly) != 0)
                                        {
                                            fMoreInfo = (uint)(tagVSQueryEditResultFlags.QER_EditNotPossible | tagVSQueryEditResultFlags.QER_ReadOnlyNotUnderScc);
                                        }
                                        else
                                        {
                                            bool fChangeAttribute = false;
                                            if ((rgfQueryEdit & (uint)tagVSQueryEditFlags.QEF_SilentMode) != 0)
                                            {
                                                // When called in silent mode, deny the edit and return QER_NoisyPromptRequired and expect for a non-silent call)
                                                // (The alternative is to silently make the file writable and accept the edit)
                                                fMoreInfo = (uint)(tagVSQueryEditResultFlags.QER_EditNotPossible | tagVSQueryEditResultFlags.QER_ReadOnlyNotUnderScc | tagVSQueryEditResultFlags.QER_NoisyPromptRequired );
                                            }
                                            else
                                            {
                                                // This is a controlled file, warn the user
                                                IVsUIShell uiShell = (IVsUIShell)_sccProvider.GetService(typeof(SVsUIShell));
                                                Guid clsid = Guid.Empty;
                                                int result = VSConstants.S_OK;
                                                string messageText = Resources.ResourceManager.GetString("QEQS_EditUncontrolledReadOnly");
                                                string messageCaption = Resources.ResourceManager.GetString("ProviderName");
                                                if (uiShell.ShowMessageBox(0, ref clsid,
                                                                    messageCaption,
                                                                    String.Format(CultureInfo.CurrentUICulture, messageText, rgpszMkDocuments[iFile]),
                                                                    string.Empty,
                                                                    0,
                                                                    OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                                                                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                                                                    OLEMSGICON.OLEMSGICON_QUERY,
                                                                    0,        // false = application modal; true would make it system modal
                                                                    out result) == VSConstants.S_OK
                                                    && result == (int)DialogResult.Yes)
                                                {
                                                    fChangeAttribute = true;
                                                }
                                            }

                                            if (fChangeAttribute)
                                            {
                                                // Make the file writable and allow the edit
                                                File.SetAttributes(rgpszMkDocuments[iFile], FileAttributes.Normal);
                                                fEditVerdict = (uint)tagVSQueryEditResult.QER_EditOK;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        fEditVerdict = (uint)tagVSQueryEditResult.QER_EditOK;
                                    }
                                    break;
                            }
                        }
                    }

                    // It's a bit unfortunate that we have to return only one set of flags for all the files involved in the operation
                    // The edit can continue if all the files were approved for edit
                    prgfMoreInfo |= fMoreInfo;
                    pfEditVerdict |= fEditVerdict;
                }
            }
            catch(Exception)
            {
                // If an exception was caught, do not allow the edit
                pfEditVerdict = (uint)tagVSQueryEditResult.QER_EditNotOK;
                prgfMoreInfo = (uint)tagVSQueryEditResultFlags.QER_EditNotPossible;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by editors and projects before saving the files
        /// The function allows the source control systems to take the necessary actions (checkout, flip attributes)
        /// to make the file writable in order to allow the file saving to continue
        /// </summary>
        public int QuerySaveFile([InAttribute] string pszMkDocument, [InAttribute] uint rgf, [InAttribute] VSQEQS_FILE_ATTRIBUTE_DATA[] pFileInfo, out uint pdwQSResult)
        {
            // Delegate to the other QuerySave function
            string[] rgszDocuements = new string[1];
            uint[] rgrgf = new uint[1];
            rgszDocuements[0] = pszMkDocument;
            rgrgf[0] = rgf;
            return QuerySaveFiles(((uint)tagVSQuerySaveFlags.QSF_DefaultOperation), 1, rgszDocuements, rgrgf, pFileInfo, out pdwQSResult);
        }

        /// <summary>
        /// Called by editors and projects before saving the files
        /// The function allows the source control systems to take the necessary actions (checkout, flip attributes)
        /// to make the file writable in order to allow the file saving to continue
        /// </summary>
        public int QuerySaveFiles([InAttribute] uint rgfQuerySave, [InAttribute] int cFiles, [InAttribute] string[] rgpszMkDocuments, [InAttribute] uint[] rgrgf, [InAttribute] VSQEQS_FILE_ATTRIBUTE_DATA[] rgFileInfo, out uint pdwQSResult)
        {
            // Initialize output variables
            // It's a bit unfortunate that we have to return only one set of flags for all the files involved in the operation
            // The last file will win setting this flag
            pdwQSResult = (uint)tagVSQuerySaveResult.QSR_SaveOK;

            // In non-UI mode attempt to silently flip the attributes of files or check them out 
            // and allow the save, because the user cannot be asked what to do with the file
            if (_sccProvider.InCommandLineMode())
            {
                rgfQuerySave = rgfQuerySave | (uint)tagVSQuerySaveFlags.QSF_SilentMode;
            }

            try 
            {
                for (int iFile = 0; iFile < cFiles; iFile++)
                {
                    SourceControlStatus status = GetFileStatus(rgpszMkDocuments[iFile]);
                    bool fileExists = File.Exists(rgpszMkDocuments[iFile]);
                    bool isFileReadOnly = false;
                    if (fileExists)
                    {
                        isFileReadOnly = ((File.GetAttributes(rgpszMkDocuments[iFile]) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
                    }

                    switch (status)
                    {
                        case SourceControlStatus.scsCheckedIn:
                            DlgQuerySaveCheckedInFile dlgAskCheckout = new DlgQuerySaveCheckedInFile(rgpszMkDocuments[iFile]);
                            if ((rgfQuerySave & (uint)tagVSQuerySaveFlags.QSF_SilentMode) != 0)
                            {
                                // When called in silent mode, attempt the checkout
                                // (The alternative is to deny the save, return QSR_NoSave_NoisyPromptRequired and expect for a non-silent call)
                                dlgAskCheckout.Answer = DlgQuerySaveCheckedInFile.qscifCheckout;
                            }
                            else
                            {
                                dlgAskCheckout.ShowDialog();
                            }

                            switch (dlgAskCheckout.Answer)
                            {
                                case DlgQueryEditCheckedInFile.qecifCheckout:
                                    // Checkout the file, and since it cannot fail, allow the save to continue
                                    CheckoutFileAndRefreshProjectGlyphs(rgpszMkDocuments[iFile]);
                                    pdwQSResult = (uint)tagVSQuerySaveResult.QSR_SaveOK;
                                    break;
                                case DlgQuerySaveCheckedInFile.qscifForceSaveAs:
                                    pdwQSResult = (uint)tagVSQuerySaveResult.QSR_ForceSaveAs;
                                    break;
                                case DlgQuerySaveCheckedInFile.qscifSkipSave:
                                    pdwQSResult = (uint)tagVSQuerySaveResult.QSR_NoSave_Continue;
                                    break;
                                default:
                                    pdwQSResult = (uint)tagVSQuerySaveResult.QSR_NoSave_Cancel;
                                    break;
                            }

                            break;
                        case SourceControlStatus.scsCheckedOut: // fall through
                        case SourceControlStatus.scsUncontrolled:
                            if (fileExists && isFileReadOnly)
                            {
                                // Make the file writable and allow the save
                                File.SetAttributes(rgpszMkDocuments[iFile], FileAttributes.Normal);
                            }
                            // Allow the save now 
                            pdwQSResult = (uint)tagVSQuerySaveResult.QSR_SaveOK;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                // If an exception was caught, do not allow the save
                pdwQSResult = (uint)tagVSQuerySaveResult.QSR_NoSave_Cancel;
            }
     
            return VSConstants.S_OK;
        }

        #endregion

        //--------------------------------------------------------------------------------
        // IVsSccSolution specific members
        //--------------------------------------------------------------------------------
        public event EventHandler AddedToSourceControl;

        //--------------------------------------------------------------------------------
        // IVsTrackProjectDocumentsEvents2 specific functions
        //--------------------------------------------------------------------------------

        public int OnQueryAddFiles([InAttribute] IVsProject pProject, [InAttribute] int cFiles, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSQUERYADDFILEFLAGS[] rgFlags, [OutAttribute] VSQUERYADDFILERESULTS[] pSummaryResult, [OutAttribute] VSQUERYADDFILERESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Implement this function to update the project scc glyphs when the items are added to the project.
        /// If a project doesn't call GetSccGlyphs as they should do (as solution folder do), this will update correctly the glyphs when the project is controled
        /// </summary>
        public int OnAfterAddFilesEx([InAttribute] int cProjects, [InAttribute] int cFiles, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSADDFILEFLAGS[] rgFlags)
        {
            // Start by iterating through all projects calling this function
            for (int iProject = 0; iProject < cProjects; iProject++)
            {
                IVsSccProject2 sccProject = rgpProjects[iProject] as IVsSccProject2;
                IVsHierarchy pHier = rgpProjects[iProject] as IVsHierarchy;

                // If the project is not controllable, or is not controlled, skip it
                if (sccProject == null || !IsProjectControlled(pHier))
                {
                    continue;
                }

                // Files in this project are in rgszMkOldNames, rgszMkNewNames arrays starting with iProjectFilesStart index and ending at iNextProjecFilesStart-1
                int iProjectFilesStart = rgFirstIndices[iProject];
                int iNextProjecFilesStart = cFiles;
                if (iProject < cProjects - 1)
                {
                    iNextProjecFilesStart = rgFirstIndices[iProject + 1];
                }

                // Now that we know which files belong to this project, iterate the project files
                for (int iFile = iProjectFilesStart; iFile < iNextProjecFilesStart; iFile++)
                {
                    // Refresh the solution explorer glyphs for all projects containing this file
                    IList<VSITEMSELECTION> nodes = GetControlledProjectsContainingFile(rgpszMkDocuments[iFile]);
                    _sccProvider.RefreshNodesGlyphs(nodes);
                }
            }

            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryAddDirectories ([InAttribute] IVsProject pProject, [InAttribute] int cDirectories, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSQUERYADDDIRECTORYFLAGS[] rgFlags, [OutAttribute] VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, [OutAttribute] VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterAddDirectoriesEx ([InAttribute] int cProjects, [InAttribute] int cDirectories, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Implement OnQueryRemoveFilesevent to warn the user when he's deleting controlled files.
        /// The user gets the chance to cancel the file removal.
        /// </summary>
        public int OnQueryRemoveFiles([InAttribute] IVsProject pProject, [InAttribute] int cFiles, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSQUERYREMOVEFILEFLAGS[] rgFlags, [OutAttribute] VSQUERYREMOVEFILERESULTS[] pSummaryResult, [OutAttribute] VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            pSummaryResult[0] = VSQUERYREMOVEFILERESULTS.VSQUERYREMOVEFILERESULTS_RemoveOK;
            if (rgResults != null)
            {
                for (int iFile = 0; iFile < cFiles; iFile++)
                {
                    rgResults[iFile] = VSQUERYREMOVEFILERESULTS.VSQUERYREMOVEFILERESULTS_RemoveOK;
                }
            }

            try
            {
                IVsSccProject2 sccProject = pProject as IVsSccProject2;
                IVsHierarchy pHier = pProject as IVsHierarchy;
                string projectName = null;
                if (sccProject == null)
                {
                    // This is the solution calling
                    pHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
                    projectName = _sccProvider.GetSolutionFileName();
                }
                else
                {
                    // If the project doesn't support source control, it will be skipped
                    if (sccProject != null)
                    {
                        projectName = _sccProvider.GetProjectFileName(sccProject);
                    }
                }
                
                if (projectName != null)
                {
                    for (int iFile = 0; iFile < cFiles; iFile++)
                    {
                        SccProviderStorage storage = _controlledProjects[pHier] as SccProviderStorage;
                        if (storage != null)
                        {
                            SourceControlStatus status = storage.GetFileStatus(rgpszMkDocuments[iFile]);
                            if (status != SourceControlStatus.scsUncontrolled)
                            {
                                // This is a controlled file, warn the user
                                IVsUIShell uiShell = (IVsUIShell)_sccProvider.GetService(typeof(SVsUIShell));
                                Guid clsid = Guid.Empty;
                                int result = VSConstants.S_OK;
                                string messageText = Resources.ResourceManager.GetString("TPD_DeleteControlledFile"); 
					            string messageCaption = Resources.ResourceManager.GetString("ProviderName");
                                if (uiShell.ShowMessageBox(0, ref clsid,
                                                    messageCaption,
                                                    String.Format(CultureInfo.CurrentUICulture, messageText, rgpszMkDocuments[iFile]),
                                                    string.Empty,
                                                    0,
                                                    OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                                                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                                                    OLEMSGICON.OLEMSGICON_QUERY,
                                                    0,        // false = application modal; true would make it system modal
                                                    out result) != VSConstants.S_OK
                                    || result != (int)DialogResult.Yes)
                                {
                                    pSummaryResult[0] = VSQUERYREMOVEFILERESULTS.VSQUERYREMOVEFILERESULTS_RemoveNotOK;
                                    if (rgResults != null)
                                    {
                                        rgResults[iFile] = VSQUERYREMOVEFILERESULTS.VSQUERYREMOVEFILERESULTS_RemoveNotOK;
                                    }
                                    // Don't spend time iterating through the rest of the files once the rename has been cancelled
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                pSummaryResult[0] = VSQUERYREMOVEFILERESULTS.VSQUERYREMOVEFILERESULTS_RemoveNotOK;
                if (rgResults != null)
                {
                    for (int iFile = 0; iFile < cFiles; iFile++)
                    {
                        rgResults[iFile] = VSQUERYREMOVEFILERESULTS.VSQUERYREMOVEFILERESULTS_RemoveNotOK;
                    }
                }
            }
            
            return VSConstants.S_OK;
        }

        public int OnAfterRemoveFiles([InAttribute] int cProjects, [InAttribute] int cFiles, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSREMOVEFILEFLAGS[] rgFlags)
        {
            // The file deletes are not propagated into the store
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryRemoveDirectories([InAttribute] IVsProject pProject, [InAttribute] int cDirectories, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, [OutAttribute] VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, [OutAttribute] VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterRemoveDirectories([InAttribute] int cProjects, [InAttribute] int cDirectories, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnQueryRenameFiles([InAttribute] IVsProject pProject, [InAttribute] int cFiles, [InAttribute] string[] rgszMkOldNames, [InAttribute] string[] rgszMkNewNames, [InAttribute] VSQUERYRENAMEFILEFLAGS[] rgFlags, [OutAttribute] VSQUERYRENAMEFILERESULTS[] pSummaryResult, [OutAttribute] VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Implement OnAfterRenameFiles event to rename a file in the source control store when it gets renamed in the project
        /// Also, rename the store if the project itself is renamed
        /// </summary>
        public int OnAfterRenameFiles([InAttribute] int cProjects, [InAttribute] int cFiles, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgszMkOldNames, [InAttribute] string[] rgszMkNewNames, [InAttribute] VSRENAMEFILEFLAGS[] rgFlags)
        {
            // Start by iterating through all projects calling this function
            for (int iProject = 0; iProject < cProjects; iProject++)
            {
                IVsSccProject2 sccProject = rgpProjects[iProject] as IVsSccProject2;
                IVsHierarchy pHier = rgpProjects[iProject] as IVsHierarchy;
                string projectName = null;
                if (sccProject == null)
                {
                    // This is the solution calling
                    pHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
                    projectName = _sccProvider.GetSolutionFileName();
                }
                else
                {
                    if (sccProject == null)
                    {
                        // It is a project that doesn't support source control, in which case it should be ignored
                        continue;
                    }

                    projectName = _sccProvider.GetProjectFileName(sccProject);
                }

                // Files in this project are in rgszMkOldNames, rgszMkNewNames arrays starting with iProjectFilesStart index and ending at iNextProjecFilesStart-1
                int iProjectFilesStart = rgFirstIndices[iProject];
                int iNextProjecFilesStart = cFiles;
                if (iProject < cProjects - 1)
                {
                    iNextProjecFilesStart = rgFirstIndices[iProject+1];
                }

                // Now that we know which files belong to this project, iterate the project files
                for (int iFile = iProjectFilesStart; iFile < iNextProjecFilesStart; iFile++)
                {
                    SccProviderStorage storage = _controlledProjects[pHier] as SccProviderStorage;
                    if (storage != null)
                    {
                        storage.RenameFileInStorage(rgszMkOldNames[iFile], rgszMkNewNames[iFile]);

                        // And refresh the solution explorer glyphs because we affected the source control status of this file
                        // Note that by now, the project should already know about the new file name being part of its hierarchy
                        IList<VSITEMSELECTION> nodes = GetControlledProjectsContainingFile(rgszMkNewNames[iFile]);
                        _sccProvider.RefreshNodesGlyphs(nodes);
                    }
                }
            }

            return VSConstants.S_OK;
        }

        public int OnQueryRenameDirectories([InAttribute] IVsProject pProject, [InAttribute] int cDirs, [InAttribute] string[] rgszMkOldNames, [InAttribute] string[] rgszMkNewNames, [InAttribute] VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, [OutAttribute] VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, [OutAttribute] VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterRenameDirectories([InAttribute] int cProjects, [InAttribute] int cDirs, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgszMkOldNames, [InAttribute] string[] rgszMkNewNames, [InAttribute] VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnAfterSccStatusChanged([InAttribute] int cProjects, [InAttribute] int cFiles, [InAttribute] IVsProject[] rgpProjects, [InAttribute] int[] rgFirstIndices, [InAttribute] string[] rgpszMkDocuments, [InAttribute] uint[] rgdwSccStatus)
        {
            return VSConstants.E_NOTIMPL;
        }

        #region Files and Project Management Functions

        /// <summary>
        /// Returns whether this source control provider is the active scc provider.
        /// </summary>
        public bool Active
        {
            get { return _active; }
        }

        /// <summary>
        /// Variable containing the solution location in source control if the solution being loaded is controlled
        /// </summary>
        public string LoadingControlledSolutionLocation
        {
            set { _loadingControlledSolutionLocation = value; }
        }

        /// <summary>
        /// Checks whether the specified project or solution (pHier==null) is under source control
        /// </summary>
        /// <returns>True if project is controlled.</returns>
        public bool IsProjectControlled(IVsHierarchy pHier)
        {
            if (pHier == null)
            {
                // this is solution, get the solution hierarchy
                pHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
            }

            return _controlledProjects.ContainsKey(pHier);
        }

        /// <summary>
        /// Checks whether the specified project or solution (pHier==null) is offline
        /// </summary>
        /// <returns>True if project is offline.</returns>
        public bool IsProjectOffline(IVsHierarchy pHier)
        {
            if (pHier == null)
            {
                // this is solution, get the solution hierarchy
                pHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
            }

            return _offlineProjects.ContainsKey(pHier);
        }

        /// <summary>
        /// Toggle the offline status of the specified project or solution
        /// </summary>
        /// <returns>True if project is offline.</returns>
        public void ToggleOfflineStatus(IVsHierarchy pHier)
        {
            if (pHier == null)
            {
                // this is solution, get the solution hierarchy
                pHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
            }

            if (_offlineProjects.ContainsKey(pHier))
            {
                _offlineProjects.Remove(pHier);
            }
            else
            {
                _offlineProjects[pHier] = true;
            }
        }

        /// <summary>
        /// Adds the specified projects and solution to source control
        /// </summary>
        public void AddProjectsToSourceControl(ref Hashtable hashUncontrolledProjects, bool addSolutionToSourceControl)
        {
            // A real source control provider will ask the user for a location where the projects will be controlled
            // From the user input it should create up to 4 strings that will pass them to the projects to persist, 
            // so next time the project is open from disk, it will callback source control package, and the package
            // could use the 4 binding strings to identify the correct database location of the project files.
            foreach (IVsHierarchy pHier in hashUncontrolledProjects.Keys)
            {
                IVsSccProject2 sccProject2 = (IVsSccProject2)pHier;
                sccProject2.SetSccLocation("<Project Location In Database>", "<Source Control Database>", "<Local Binding Root of Project>", _sccProvider.ProviderName);

                // Add the newly controlled projects now to the list of controlled projects in this solution
                _controlledProjects[pHier] = null;
            }

            // Also, if the solution was selected to be added to scc, write in the solution properties the controlled status
            if (addSolutionToSourceControl)
            {
                IVsHierarchy solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
                _controlledProjects[solHier] = null;
                _sccProvider.SolutionHasDirtyProps = true;
            }

            // Now save all the modified files
            IVsSolution sol = (IVsSolution)_sccProvider.GetService(typeof(SVsSolution));
            sol.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty, null, 0);

            // Add now the solution and project files to the source control database
            // which in our case means creating a text file containing the list of controlled files
            foreach (IVsHierarchy pHier in hashUncontrolledProjects.Keys)
            {
                IVsSccProject2 sccProject2 = (IVsSccProject2)pHier;
                IList<string> files = _sccProvider.GetProjectFiles(sccProject2);
                SccProviderStorage storage = new SccProviderStorage(_sccProvider.GetProjectFileName(sccProject2));
                storage.AddFilesToStorage(files);
                _controlledProjects[pHier] = storage;
            }

            // If adding solution to source control, create a storage for the solution, too
            if (addSolutionToSourceControl)
            {
                IVsHierarchy solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
                IList<string> files = new List<string>();
                string solutionFile = _sccProvider.GetSolutionFileName();
                files.Add(solutionFile);
                SccProviderStorage storage = new SccProviderStorage(solutionFile);
                storage.AddFilesToStorage(files);
                _controlledProjects[solHier] = storage;
            }

            // For all the projects added to source control, refresh their source control glyphs
            IList<VSITEMSELECTION> nodes = new List<VSITEMSELECTION>();
            foreach (IVsHierarchy pHier in hashUncontrolledProjects.Keys)
            {
                VSITEMSELECTION vsItem;
                vsItem.itemid = VSConstants.VSITEMID_ROOT;
                vsItem.pHier = pHier;
                nodes.Add(vsItem);
            }

            // Also, add the solution if necessary to the list of glyphs to refresh
            if (addSolutionToSourceControl)
            {
                VSITEMSELECTION vsItem;
                vsItem.itemid = VSConstants.VSITEMID_ROOT;
                vsItem.pHier = null;
                nodes.Add(vsItem);
            }

            _sccProvider.RefreshNodesGlyphs(nodes);

            // Raise the event to inform the shell that the solution was added to Source Control
            AddedToSourceControl?.Invoke(this, EventArgs.Empty);
        }

        // The following methods are not very efficient
        // A good source control provider should maintain maps to identify faster to which project does a file belong
        // and check only the status of the files in that project; or simply, query one common storage about the file status

        /// <summary>
        /// Returns the source control status of the specified file
        /// </summary>
        public SourceControlStatus GetFileStatus(string filename)
        {
            foreach (SccProviderStorage storage in _controlledProjects.Values)
            {
                if (storage != null)
                {
                    SourceControlStatus status = storage.GetFileStatus(filename);
                    if (status != SourceControlStatus.scsUncontrolled)
                    {
                        return status;
                    }
                }
            }

            return SourceControlStatus.scsUncontrolled;
        }

        /// <summary>
        /// Adds the specified file to source control; the file must be part of a controlled project
        /// </summary>
        public void AddFileToSourceControl(string file)
        {
            IList<string> filesToAdd = new List<string>();
            filesToAdd.Add(file);
            // Get all controlled projects containing this file
            IList<VSITEMSELECTION> nodes = GetControlledProjectsContainingFile(file);
            foreach (VSITEMSELECTION vsItem in nodes)
            {
                SccProviderStorage storage = _controlledProjects[vsItem.pHier] as SccProviderStorage;
                if (storage != null)
                {
                    storage.AddFilesToStorage(filesToAdd);
                }
            }
        }

        /// <summary>
        /// Checks in the specified file
        /// </summary>
        public void CheckinFile(string file)
        {
            // Before checking in files, make sure all in-memory edits have been commited to disk 
            // by forcing a save of the solution. Ideally, only the files to be checked in should be saved...
            IVsSolution sol = (IVsSolution)_sccProvider.GetService(typeof(SVsSolution));
            if (sol.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty, null, 0) != VSConstants.S_OK)
            {
                // If saving the files failed, don't continue with the checkin
                return;
            }

            foreach (SccProviderStorage storage in _controlledProjects.Values)
            {
                if (storage != null)
                {
                    SourceControlStatus status = storage.GetFileStatus(file);
                    if (status != SourceControlStatus.scsUncontrolled)
                    {
                        storage.CheckinFile(file);

                        // Decrease the pending change count since the file was checked in
                        PendingChangeCount--;

                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Checkout the specified file from source control
        /// </summary>
        public void CheckoutFile(string file)
        {
            foreach (SccProviderStorage storage in _controlledProjects.Values)
            {
                if (storage != null)
                {
                    SourceControlStatus status = storage.GetFileStatus(file);
                    if (status != SourceControlStatus.scsUncontrolled)
                    {
                        storage.CheckoutFile(file);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of controlled projects containing the specified file
        /// </summary>
        public IList<VSITEMSELECTION> GetControlledProjectsContainingFile(string file)
        {
            // Accumulate all the controlled projects that contain this file
            IList<VSITEMSELECTION> nodes = new List<VSITEMSELECTION>();

            foreach (IVsHierarchy pHier in _controlledProjects.Keys)
            {
                IVsHierarchy solHier = (IVsHierarchy)_sccProvider.GetService(typeof(SVsSolution));
                if (solHier == pHier)
                {
                    // This is the solution
                    if (file.ToLower().CompareTo(_sccProvider.GetSolutionFileName().ToLower()) == 0)
                    {
                        VSITEMSELECTION vsItem;
                        vsItem.itemid = VSConstants.VSITEMID_ROOT;
                        vsItem.pHier = null;
                        nodes.Add(vsItem);
                    }
                }
                else
                {
                    IVsProject2 pProject = pHier as IVsProject2;
                    // See if the file is member of this project
                    // Caveat: the IsDocumentInProject function is expensive for certain project types, 
                    // you may want to limit its usage by creating your own maps of file2project or folder2project
                    int fFound;
                    uint itemid;
                    VSDOCUMENTPRIORITY[] prio = new VSDOCUMENTPRIORITY[1];
                    if (pProject != null && pProject.IsDocumentInProject(file, out fFound, prio, out itemid) == VSConstants.S_OK && fFound != 0)
                    {
                        VSITEMSELECTION vsItem;
                        vsItem.itemid = itemid;
                        vsItem.pHier = pHier;
                        nodes.Add(vsItem);
                    }
                }
            }

            return nodes;
        }

        /// <summary>
        /// Checkout the file from source control and refreshes the glyphs of the files containing the file
        /// </summary>
        public void CheckoutFileAndRefreshProjectGlyphs(string file)
        {
            // First, checkout the file
            CheckoutFile(file);

            // And refresh the solution explorer glyphs of all the projects containing this file to reflect the checked out status
            IList<VSITEMSELECTION> nodes = GetControlledProjectsContainingFile(file);
            _sccProvider.RefreshNodesGlyphs(nodes);
        }

        #endregion
    }
}