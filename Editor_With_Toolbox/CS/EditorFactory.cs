/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.Samples.VisualStudio.IDE.EditorWithToolbox
{
    /// <summary>
    /// Factory for creating our editors.
    /// </summary>
    [Guid(GuidStrings.GuidEditorFactory)]
    public class EditorFactory : IVsEditorFactory, IDisposable
    {
        #region Fields
        // private instance of the EditorFactory's OleServiceProvider
        private ServiceProvider vsServiceProvider;
        #endregion

        #region Constructors
        /// <summary>
        /// Explicitly defined default constructor.
        /// Initialize new instance of the EditorFactory object.
        /// </summary>
        public EditorFactory()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering {0} constructor", typeof(EditorFactory).ToString()));
        }

        #endregion Constructors

        #region Methods
		
        #region IDisposable Pattern implementation
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">This parameter determines whether the method has been called directly or indirectly by a user's code.</param>
        private void Dispose(bool disposing)
        {
            // If disposing equals true, dispose all managed and unmanaged resources
            if (disposing)
            {
                /// Since we create a ServiceProvider which implements IDisposable we
                /// also need to implement IDisposable to make sure that the ServiceProvider's
                /// Dispose method gets called.
                if (vsServiceProvider != null)
                {
                    vsServiceProvider.Dispose();
                    vsServiceProvider = null;
                }
            }
        }
        #endregion

        #region IVsEditorFactory Members

        /// <summary>
        /// Used for initialization of the editor in the environment.
        /// </summary>  
        /// <param name="psp">Pointer to the service provider. Can be used to obtain instances of other interfaces.</param>
        /// <returns>S_OK if the method succeeds.</returns>
        public int SetSite(IOleServiceProvider psp)
        {
            vsServiceProvider = new ServiceProvider(psp);
            return VSConstants.S_OK;
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
		//		HKLM\Software\Microsoft\VisualStudio\10.0\Editors\
		//			{...guidEditor...}\
		//				LogicalViews\
		//					{...LOGVIEWID_TextView...} = s ''
		//					{...LOGVIEWID_Code...} = s ''
		//					{...LOGVIEWID_Debugging...} = s ''
		//					{...LOGVIEWID_Designer...} = s 'Form'
		//
		public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
		{
			pbstrPhysicalView = null;	// initialize out parameter

			// we support only a single physical view
            if (VSConstants.LOGVIEWID_Primary == rguidLogicalView)
            {
                // primary view uses NULL as pbstrPhysicalView
                return VSConstants.S_OK;
            }
            else
            {
                // you must return E_NOTIMPL for any unrecognized rguidLogicalView values
                return VSConstants.E_NOTIMPL;
            }
		}
        /// <summary>
        /// Releases all cached interface pointers and unregisters any event sinks
        /// </summary>
        /// <returns>S_OK if the method succeeds.</returns>
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
        /// are valid.
        /// </param>
        /// <param name="pszMkDocument">path to the file to be opened.</param>
        /// <param name="pszPhysicalView">name of the physical view.</param>
        /// <param name="pvHier">pointer to the IVsHierarchy interface.</param>
        /// <param name="itemid">Item identifier of this editor instance.</param>
        /// <param name="punkDocDataExisting">This parameter is used to determine if a document buffer 
        /// (DocData object) has already been created.
        /// </param>
        /// <param name="ppunkDocView">Pointer to the IUnknown interface for the DocView object.</param>
        /// <param name="ppunkDocData">Pointer to the IUnknown interface for the DocData object.</param>
        /// <param name="pbstrEditorCaption">Caption mentioned by the editor for the doc window.</param>
        /// <param name="pguidCmdUI">the Command UI Guid. Any UI element that is visible in the editor has 
        /// to use this GUID.
        /// </param>
        /// <param name="pgrfCDW">Flags for CreateDocumentWindow.</param>
        /// <returns>HRESULT result code. S_OK if the method succeeds.</returns>
        /// <remarks>
        /// Attribute usage according to FxCop rule regarding SecurityAction requirements (LinkDemand).
        /// This method do use SecurityAction.Demand action instead of LinkDemand because it overrides method without LinkDemand
        /// see "Demand vs. LinkDemand" article in MSDN for more details.
        /// </remarks>
        [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
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
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering {0} CreateEditorInstance()", ToString()));

            // Initialize to null
            ppunkDocView = IntPtr.Zero;
            ppunkDocData = IntPtr.Zero;
            pguidCmdUI = GuidList.guidEditorFactory;
            pgrfCDW = 0;
            pbstrEditorCaption = null;

            // Validate inputs
            if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }
            if (punkDocDataExisting != IntPtr.Zero)
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            // Create the Document (editor)
            EditorPane newEditor = new EditorPane();
            ppunkDocView = Marshal.GetIUnknownForObject(newEditor);
            ppunkDocData = Marshal.GetIUnknownForObject(newEditor);
            pbstrEditorCaption = "";

            return VSConstants.S_OK;
        }

        #endregion

        #region Other methods

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An Type that specifies the type of service object to get.</param>
        /// <returns>A service object of type serviceType. -or- 
        /// A a null reference if there is no service object of type serviceType.</returns>
        public object GetService(Type serviceType)
        {
            return vsServiceProvider.GetService(serviceType);
        }

        #endregion 

        #endregion
    }
}
