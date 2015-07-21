/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VsTemplateDesigner
{
    /// <summary>
    /// Factory for creating our editor object. Extends from the IVsEditoryFactory interface
    /// </summary>
    [Guid(GuidList.guidVsTemplateDesignerEditorFactoryString)]
    public sealed class EditorFactory : IVsEditorFactory, IDisposable
    {
        public const string Extension = ".vstemplate";

        private VsTemplateDesignerPackage editorPackage;
        private ServiceProvider vsServiceProvider;


        public EditorFactory(VsTemplateDesignerPackage package)
        {
            editorPackage = package;
        }

        #region IVsEditorFactory Members

        /// <summary>
        /// Used for initialization of the editor in the environment
        /// </summary>
        /// <param name="psp">pointer to the service provider. Can be used to obtain instances of other interfaces
        /// </param>
        /// <returns></returns>
        public int SetSite(IOleServiceProvider psp)
        {
            vsServiceProvider = new ServiceProvider(psp);
            return VSConstants.S_OK;
        }

        public object GetService(Type serviceType)
        {
            return vsServiceProvider.GetService(serviceType);
        }

        // This method is called by the Environment (inside IVsUIShellOpenDocument::
        // OpenStandardEditor and OpenSpecificEditor) to map a LOGICAL view to a 
        // PHYSICAL view. A LOGICAL view identifies the purpose of the view that is
        // desired (e.g. a view appropriate for Debugging [LOGVIEWID_Debugging], or a 
        // view appropriate for text view manipulation as by navigating to a find
        // result [LOGVIEWID_TextView]). A PHYSICAL view identifies an actual type 
        // of view implementation that an IVsEditorFactory can create. 
        //
        // NOTE: Physical views are identified by a string of your choice with the 
        // one constraint that the default/primary physical view for an editor  
        // *MUST* use a NULL string as its physical view name (*pbstrPhysicalView = NULL).
        //
        // NOTE: It is essential that the implementation of MapLogicalView properly
        // validates that the LogicalView desired is actually supported by the editor.
        // If an unsupported LogicalView is requested then E_NOTIMPL must be returned.
        //
        // NOTE: The special Logical Views supported by an Editor Factory must also 
        // be registered in the local registry hive. LOGVIEWID_Primary is implicitly 
        // supported by all editor types and does not need to be registered.
        // For example, an editor that supports a ViewCode/ViewDesigner scenario
        // might register something like the following:
        //        HKLM\Software\Microsoft\VisualStudio\<version>\Editors\
        //            {...guidEditor...}\
        //                LogicalViews\
        //                    {...LOGVIEWID_TextView...} = s ''
        //                    {...LOGVIEWID_Code...} = s ''
        //                    {...LOGVIEWID_Debugging...} = s ''
        //                    {...LOGVIEWID_Designer...} = s 'Form'
        //
        public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            pbstrPhysicalView = null;    // initialize out parameter

            // we support only a single physical view
            if (VSConstants.LOGVIEWID_Primary == rguidLogicalView)
                return VSConstants.S_OK;        // primary view uses NULL as pbstrPhysicalView
            else
                return VSConstants.E_NOTIMPL;   // you must return E_NOTIMPL for any unrecognized rguidLogicalView values
        }

        public int Close()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Used by the editor factory to create an editor instance. the environment first determines the 
        /// editor factory with the highest priority for opening the file and then calls 
        /// IVsEditorFactory.CreateEditorInstance. If the environment is unable to instantiate the document data 
        /// in that editor, it will find the editor with the next highest priority and attempt to so that same 
        /// thing. 
        /// NOTE: The priority of our editor is 32 as mentioned in the attributes on the package class.
        /// 
        /// Since our editor supports opening only a single view for an instance of the document data, if we 
        /// are requested to open document data that is already instantiated in another editor, or even our 
        /// editor, we return a value VS_E_INCOMPATIBLEDOCDATA.
        /// </summary>
        /// <param name="grfCreateDoc">Flags determining when to create the editor. Only open and silent flags 
        /// are valid
        /// </param>
        /// <param name="pszMkDocument">path to the file to be opened</param>
        /// <param name="pszPhysicalView">name of the physical view</param>
        /// <param name="pvHier">pointer to the IVsHierarchy interface</param>
        /// <param name="itemid">Item identifier of this editor instance</param>
        /// <param name="punkDocDataExisting">This parameter is used to determine if a document buffer 
        /// (DocData object) has already been created
        /// </param>
        /// <param name="ppunkDocView">Pointer to the IUnknown interface for the DocView object</param>
        /// <param name="ppunkDocData">Pointer to the IUnknown interface for the DocData object</param>
        /// <param name="pbstrEditorCaption">Caption mentioned by the editor for the doc window</param>
        /// <param name="pguidCmdUI">the Command UI Guid. Any UI element that is visible in the editor has 
        /// to use this GUID. This is specified in the .vsct file
        /// </param>
        /// <param name="pgrfCDW">Flags for CreateDocumentWindow</param>
        /// <returns></returns>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public int CreateEditorInstance(
                        uint grfCreateDoc,
                        string pszMkDocument,
                        string pszPhysicalView,
                        IVsHierarchy pvHier,
                        uint itemid,
                        IntPtr punkDocDataExisting,
                        out IntPtr ppunkDocView,
                        out IntPtr ppunkDocData,
                        out string pbstrEditorCaption,
                        out Guid pguidCmdUI,
                        out int pgrfCDW)
        {
            // Initialize to null
            ppunkDocView = IntPtr.Zero;
            ppunkDocData = IntPtr.Zero;
            pguidCmdUI = GuidList.guidVsTemplateDesignerEditorFactory;
            pgrfCDW = 0;
            pbstrEditorCaption = null;

            // Validate inputs
            if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }

            IVsTextLines textBuffer = null;

            if (punkDocDataExisting == IntPtr.Zero)
            {
                // punkDocDataExisting is null which means the file is not yet open.
                // We need to create a new text buffer object 

                // get the ILocalRegistry interface so we can use it to
                // create the text buffer from the shell's local registry
                try
                {
                    ILocalRegistry localRegistry = (ILocalRegistry)GetService(typeof(SLocalRegistry));
                    if (localRegistry != null)
                    {
                        IntPtr ptr;
                        Guid iid = typeof(IVsTextLines).GUID;
                        Guid CLSID_VsTextBuffer = typeof(VsTextBufferClass).GUID;
                        localRegistry.CreateInstance(CLSID_VsTextBuffer, null, ref iid, 1 /*CLSCTX_INPROC_SERVER*/, out ptr);
                        try
                        {
                            textBuffer = Marshal.GetObjectForIUnknown(ptr) as IVsTextLines;
                        }
                        finally
                        {
                            Marshal.Release(ptr); // Release RefCount from CreateInstance call
                        }

                        // It is important to site the TextBuffer object
                        IObjectWithSite objWSite = (IObjectWithSite)textBuffer;
                        if (objWSite != null)
                        {
                            IOleServiceProvider oleServiceProvider = (IOleServiceProvider)GetService(typeof(IOleServiceProvider));
                            objWSite.SetSite(oleServiceProvider);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Can not get IVsCfgProviderEventsHelper" + ex.Message);
                    throw;
                }
            }
            else
            {
                // punkDocDataExisting is *not* null which means the file *is* already open. 
                // We need to verify that the open document is in fact a TextBuffer. If not
                // then we need to return the special error code VS_E_INCOMPATIBLEDOCDATA which
                // causes the user to be prompted to close the open file. If the user closes the
                // file then we will be called again with punkDocDataExisting as null

                // QI existing buffer for text lines
                textBuffer = Marshal.GetObjectForIUnknown(punkDocDataExisting) as IVsTextLines;
                if (textBuffer == null)
                {
                    return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
                }
            }

            // Create the Document (editor)
            EditorPane NewEditor = new EditorPane(editorPackage, pszMkDocument, textBuffer);
            ppunkDocView = Marshal.GetIUnknownForObject(NewEditor);
            ppunkDocData = Marshal.GetIUnknownForObject(textBuffer);
            pbstrEditorCaption = "";
            return VSConstants.S_OK;
        }

        #endregion

        #region IDisposable Members
     
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// This method performs instance resources clean up
        /// </summary>
        /// <param name="disposing">This parameter determines whether the method has been called directly or indirectly by a user's code</param>
        private void Dispose(bool disposing)
        {
            lock (this)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources
                if (disposing)
                {
                    if (vsServiceProvider != null)
                    {
                        vsServiceProvider.Dispose();
                        vsServiceProvider = null;
                    }
                }
            }
        }
        #endregion
    }
}
