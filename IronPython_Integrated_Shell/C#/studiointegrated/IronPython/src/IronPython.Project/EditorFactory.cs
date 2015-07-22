/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
    /// <summary>
    /// Factory for creating our editor
    /// </summary>
    [Guid("F26A6EE9-3CE6-4885-B62D-382F4DD3A50E")]
    public class EditorFactory : IVsEditorFactory
    {
        #region const
        internal static readonly Guid guidEditorFactory = new Guid("{F26A6EE9-3CE6-4885-B62D-382F4DD3A50E}");
        #endregion

        #region fields
        private PythonProjectPackage package;
        private ServiceProvider serviceProvider;
        #endregion

        #region ctors
        public EditorFactory(PythonProjectPackage package)
        {
            this.package = package;
        }
        #endregion

        #region IVsEditorFactory Members

        public virtual int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
        {
            serviceProvider = new ServiceProvider(psp);
            return VSConstants.S_OK;
        }

        public virtual object GetService(Type serviceType)
        {
            return serviceProvider.GetService(serviceType);
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
        //        HKLM\Software\Microsoft\VisualStudio\9.0\Editors\
        //            {...guidEditor...}\
        //                LogicalViews\
        //                    {...LOGVIEWID_TextView...} = s ''
        //                    {...LOGVIEWID_Code...} = s ''
        //                    {...LOGVIEWID_Debugging...} = s ''
        //                    {...LOGVIEWID_Designer...} = s 'Form'
        //
        public virtual int MapLogicalView(ref Guid logicalView, out string physicalView)
        {
            // initialize out parameter
            physicalView = null;

            bool isSupportedView = false;
            // Determine the physical view
            if (VSConstants.LOGVIEWID_Primary == logicalView)
            {
                // primary view uses NULL as pbstrPhysicalView
                isSupportedView = true;
            }
            else if (VSConstants.LOGVIEWID_Designer == logicalView)
            {
                physicalView = "Design";
                isSupportedView = true;
            }

            if (isSupportedView)
                return VSConstants.S_OK;
            else
            {
                // E_NOTIMPL must be returned for any unrecognized rguidLogicalView values
                return VSConstants.E_NOTIMPL;
            }
        }

        public virtual int Close()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grfCreateDoc"></param>
        /// <param name="pszMkDocument"></param>
        /// <param name="pszPhysicalView"></param>
        /// <param name="pvHier"></param>
        /// <param name="itemid"></param>
        /// <param name="punkDocDataExisting"></param>
        /// <param name="ppunkDocView"></param>
        /// <param name="ppunkDocData"></param>
        /// <param name="pbstrEditorCaption"></param>
        /// <param name="pguidCmdUI"></param>
        /// <param name="pgrfCDW"></param>
        /// <returns></returns>
        public virtual int CreateEditorInstance(
                        uint createEditorFlags,
                        string documentMoniker,
                        string physicalView,
                        IVsHierarchy hierarchy,
                        uint itemid,
                        System.IntPtr docDataExisting,
                        out System.IntPtr docView,
                        out System.IntPtr docData,
                        out string editorCaption,
                        out Guid commandUIGuid,
                        out int createDocumentWindowFlags)
        {
            // Initialize output parameters
            docView = IntPtr.Zero;
            docData = IntPtr.Zero;
            commandUIGuid = guidEditorFactory;
            createDocumentWindowFlags = 0;
            editorCaption = null;

            // Validate inputs
            if ((createEditorFlags & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }

            // Get a text buffer
            IVsTextLines textLines = GetTextBuffer(docDataExisting);

            // Assign docData IntPtr to either existing docData or the new text buffer
            if (docDataExisting != IntPtr.Zero)
            {
                docData = docDataExisting;
                Marshal.AddRef(docData);
            }
            else
            {
                docData = Marshal.GetIUnknownForObject(textLines);
            }

            try
            {
                docView = CreateDocumentView(physicalView, hierarchy, itemid, textLines, out editorCaption, out commandUIGuid);
            }
            finally
            {
                if (docView == IntPtr.Zero)
                {
                    if (docDataExisting != docData && docData != IntPtr.Zero)
                    {
                        // Cleanup the instance of the docData that we have addref'ed
                        Marshal.Release(docData);
                        docData = IntPtr.Zero;
                    }
                }
            }
            return VSConstants.S_OK;
        }

        
        #endregion

        #region Helper methods
        private IVsTextLines GetTextBuffer(System.IntPtr docDataExisting)
        {
            IVsTextLines textLines;
            if (docDataExisting == IntPtr.Zero)
            {
                // Create a new IVsTextLines buffer.
                Type textLinesType = typeof(IVsTextLines);
                Guid riid = textLinesType.GUID;
                Guid clsid = typeof(VsTextBufferClass).GUID;
                textLines = package.CreateInstance(ref clsid, ref riid, textLinesType) as IVsTextLines;

                // set the buffer's site
                ((IObjectWithSite)textLines).SetSite(serviceProvider.GetService(typeof(IOleServiceProvider)));
            }
            else
            {
                // Use the existing text buffer
                Object dataObject = Marshal.GetObjectForIUnknown(docDataExisting);
                textLines = dataObject as IVsTextLines;
                if (textLines == null)
                {
                    // Try get the text buffer from textbuffer provider
                    IVsTextBufferProvider textBufferProvider = dataObject as IVsTextBufferProvider;
                    if (textBufferProvider != null)
                    {
                        textBufferProvider.GetTextBuffer(out textLines);
                    }
                }
                if (textLines == null)
                {
                    // Unknown docData type then, so we have to force VS to close the other editor.
                    ErrorHandler.ThrowOnFailure((int)VSConstants.VS_E_INCOMPATIBLEDOCDATA);
                }

            }
            return textLines;
        }

        private IntPtr CreateDocumentView(string physicalView, IVsHierarchy hierarchy, uint itemid, IVsTextLines textLines, out string editorCaption, out Guid cmdUI)
        {
            //Init out params
            editorCaption = string.Empty;
            cmdUI = Guid.Empty;
            
            if (string.IsNullOrEmpty(physicalView))
            {
                // create code window as default physical view
                return CreateCodeView(textLines, ref editorCaption, ref cmdUI);
            }
            else if (string.Compare(physicalView, "design", true, CultureInfo.InvariantCulture) == 0)
            {
                // Create Form view
                return CreateFormView(hierarchy, itemid, textLines, ref editorCaption, ref cmdUI);
            }

            // We couldn't create the view
            // Return special error code so VS can try another editor factory.
            ErrorHandler.ThrowOnFailure((int)VSConstants.VS_E_UNSUPPORTEDFORMAT);

            return IntPtr.Zero;
        }

        private IntPtr CreateFormView(IVsHierarchy hierarchy, uint itemid, IVsTextLines textLines, ref string editorCaption, ref Guid cmdUI)
        {
            // Request the Designer Service
            IVSMDDesignerService designerService = (IVSMDDesignerService)GetService(typeof(IVSMDDesignerService));
        
            // Create loader for the designer
            IVSMDDesignerLoader designerLoader = (IVSMDDesignerLoader)designerService.CreateDesignerLoader("Microsoft.VisualStudio.Designer.Serialization.VSDesignerLoader");
            
            bool loaderInitalized = false;
            try
            {
                IOleServiceProvider provider = serviceProvider.GetService(typeof(IOleServiceProvider)) as IOleServiceProvider;
                // Initialize designer loader 
                designerLoader.Initialize(provider, hierarchy, (int)itemid, textLines);
                loaderInitalized = true;

                // Create the designer
                IVSMDDesigner designer = designerService.CreateDesigner(provider, designerLoader);

                // Get editor caption
                editorCaption = designerLoader.GetEditorCaption((int)READONLYSTATUS.ROSTATUS_Unknown);

                // Get view from designer
                object docView = designer.View;

                // Get command guid from designer
                cmdUI = designer.CommandGuid;

                return Marshal.GetIUnknownForObject(docView);

            }
            catch
            {
                // The designer loader may have created a reference to the shell or the text buffer.
                // In case we fail to create the designer we should manually dispose the loader
                // in order to release the references to the shell and the textbuffer
                if (loaderInitalized)
                {
                    designerLoader.Dispose();
                }
                throw;
            }
        }

        private IntPtr CreateCodeView(IVsTextLines textLines, ref string editorCaption, ref Guid cmdUI)
        {
            Type codeWindowType = typeof(IVsCodeWindow);
            Guid riid = codeWindowType.GUID;
            Guid clsid = typeof(VsCodeWindowClass).GUID;
            IVsCodeWindow window = (IVsCodeWindow)package.CreateInstance(ref clsid, ref riid, codeWindowType);
            ErrorHandler.ThrowOnFailure(window.SetBuffer(textLines));
            ErrorHandler.ThrowOnFailure(window.SetBaseEditorCaption(null));
            ErrorHandler.ThrowOnFailure(window.GetEditorCaption(READONLYSTATUS.ROSTATUS_Unknown, out editorCaption));
            cmdUI = VSConstants.GUID_TextEditorFactory;
            return Marshal.GetIUnknownForObject(window);
        }

        #endregion


    }
}
