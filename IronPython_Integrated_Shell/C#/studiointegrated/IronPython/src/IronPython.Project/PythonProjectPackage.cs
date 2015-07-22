/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using Microsoft.Samples.VisualStudio.IronPython.Project.WPFProviders;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Package;
using EnvDTE;
using Microsoft.Samples.VisualStudio.IronPython.Project.Library;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
	//Set the projectsTemplatesDirectory to a non-existant path to prevent VS from including the working directory as a valid template path
	[ProvideProjectFactory(typeof(PythonProjectFactory), "IronPython", "IronPython Project Files (*.pyproj);*.pyproj", "pyproj", "pyproj", ".\\NullPath", LanguageVsTemplate = "IronPython")]
	//Register the WPF Python Factory
	[ProvideProjectFactory(typeof(PythonWPFProjectFactory), null, null, null, null, null, LanguageVsTemplate = "IronPython", TemplateGroupIDsVsTemplate = "WPF", ShowOnlySpecifiedTemplatesVsTemplate = false)]
	[SingleFileGeneratorSupportRegistrationAttribute(typeof(PythonProjectFactory))]
	[WebSiteProject("IronPython", "Iron Python")]
    [ProvideObject(typeof(GeneralPropertyPage))]
	[ProvideObject(typeof(IronPythonBuildPropertyPage))]
	[ProvideMenuResource(1000, 1)]
    [ProvideEditorExtensionAttribute(typeof(EditorFactory), ".py", 32)]
    [ProvideEditorLogicalView(typeof(EditorFactory), "{7651a702-06e5-11d1-8ebd-00a0c90f26ea}")]  //LOGVIEWID_Designer
    [ProvideEditorLogicalView(typeof(EditorFactory), "{7651a701-06e5-11d1-8ebd-00a0c90f26ea}")]  //LOGVIEWID_Code
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[Guid(GuidList.guidPythonProjectPkgString)]
	//The following attributes are specific to sunpporting Web Application Projects
	[WAProvideProjectFactory(typeof(WAPythonProjectFactory), "IronPython Web Application Project Templates", "IronPython", false, "Web", null)]
	[WAProvideProjectFactoryTemplateMapping("{" + GuidList.guidPythonProjectFactoryString + "}", typeof(WAPythonProjectFactory))]
	[WAProvideLanguageProperty(typeof(WAPythonProjectFactory), "CodeFileExtension", ".py")]
	[WAProvideLanguageProperty(typeof(WAPythonProjectFactory), "TemplateFolder", "IronPython")]
	// IronPython does not need a CodeBehindCodeGenerator because all the code should be inline, so we disable
	// it setting a null GUID for the class that is supposed to implement it.
	[WAProvideLanguageProperty(typeof(WAPythonProjectFactory), "CodeBehindCodeGenerator", "{00000000-0000-0000-0000-000000000000}")]
    [ProvideMSBuildTargets("IronPythonCompilerTasks", @"$PackageFolder$\IronPython.targets")]
    [ProvideBindingPathAttribute]
    [RegisterSnippetsAttribute(GuidList.guidIronPythonLanguageString, false, 131,
    "IronPython", @"CodeSnippets\SnippetsIndex.xml", @"CodeSnippets\Snippets\", @"CodeSnippets\Snippets\")]
    [ProvideLanguageService(GuidList.guidIronPythonLanguageString, "IronPython", 101, RequestStockColors = true)]
    [ProvideLanguageExtension(GuidList.guidIronPythonLanguageString, ".py")]
    public class PythonProjectPackage : ProjectPackage, IVsInstalledProduct, IOleComponent
	{
        private PythonLibraryManager libraryManager;
        private uint componentID;

		protected override void Initialize()
		{
            DTE dte = (DTE)GetService(typeof(DTE));
            if (dte != null)
            {
                base.Initialize();
                this.RegisterProjectFactory(new PythonProjectFactory(this));
                this.RegisterProjectFactory(new PythonWPFProjectFactory(this as System.IServiceProvider));
                this.RegisterEditorFactory(new EditorFactory(this));
            }

            IServiceContainer container = this as IServiceContainer;
            container.AddService(typeof(IPythonLibraryManager), CreateService, true);
		}

        private object CreateService(IServiceContainer container, Type serviceType)
        {
            object service = null;
            if(typeof(IPythonLibraryManager) == serviceType)
            {
                libraryManager = new PythonLibraryManager(this);
                service = libraryManager as IPythonLibraryManager;
                RegisterForIdleTime();
            }
            return service;
        }

        private void RegisterForIdleTime()
        {
            IOleComponentManager mgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
            if (componentID == 0 && mgr != null)
            {
                OLECRINFO[] crinfo = new OLECRINFO[1];
                crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime |
                                              (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
                crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal |
                                              (uint)_OLECADVF.olecadvfRedrawOff |
                                              (uint)_OLECADVF.olecadvfWarningsOff;
                crinfo[0].uIdleTimeInterval = 1000;
                int hr = mgr.FRegisterComponent(this, crinfo, out componentID);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (componentID != 0)
                {
                    IOleComponentManager mgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
                    if (mgr != null)
                    {
                        mgr.FRevokeComponent(componentID);
                    }
                    componentID = 0;
                }
                if (null != libraryManager)
                {
                    libraryManager.Dispose();
                    libraryManager = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }


        #region IVsInstalledProduct Members
		/// <summary>
		/// This method is called during Devenv /Setup to get the bitmap to
		/// display on the splash screen for this package.
		/// </summary>
		/// <param name="pIdBmp">The resource id corresponding to the bitmap to display on the splash screen</param>
		/// <returns>HRESULT, indicating success or failure</returns>
		public int IdBmpSplash(out uint pIdBmp)
		{
			pIdBmp = 300;

			return VSConstants.S_OK;
		}

		/// <summary>
		/// This method is called to get the icon that will be displayed in the
		/// Help About dialog when this package is selected.
		/// </summary>
		/// <param name="pIdIco">The resource id corresponding to the icon to display on the Help About dialog</param>
		/// <returns>HRESULT, indicating success or failure</returns>
		public int IdIcoLogoForAboutbox(out uint pIdIco)
		{
			pIdIco = 400;

			return VSConstants.S_OK;
		}

		/// <summary>
		/// This methods provides the product official name, it will be
		/// displayed in the help about dialog.
		/// </summary>
		/// <param name="pbstrName">Out parameter to which to assign the product name</param>
		/// <returns>HRESULT, indicating success or failure</returns>
		public int OfficialName(out string pbstrName)
		{
			pbstrName = GetResourceString("@ProductName");
			return VSConstants.S_OK;
		}

		/// <summary>
		/// This methods provides the product description, it will be
		/// displayed in the help about dialog.
		/// </summary>
		/// <param name="pbstrProductDetails">Out parameter to which to assign the description of the package</param>
		/// <returns>HRESULT, indicating success or failure</returns>
		public int ProductDetails(out string pbstrProductDetails)
		{
			pbstrProductDetails = GetResourceString("@ProductDetails");
			return VSConstants.S_OK;
		}

		/// <summary>
		/// This methods provides the product version, it will be
		/// displayed in the help about dialog.
		/// </summary>
		/// <param name="pbstrPID">Out parameter to which to assign the version number</param>
		/// <returns>HRESULT, indicating success or failure</returns>
		public int ProductID(out string pbstrPID)
		{
			pbstrPID = GetResourceString("@ProductID");
			return VSConstants.S_OK;
		}

		#endregion

		/// <summary>
		/// This method loads a localized string based on the specified resource.
		/// </summary>
		/// <param name="resourceName">Resource to load</param>
		/// <returns>String loaded for the specified resource</returns>
		public string GetResourceString(string resourceName)
		{
			string resourceValue;
			IVsResourceManager resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
			if(resourceManager == null)
			{
				throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
			}
			Guid packageGuid = this.GetType().GUID;
			int hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
			Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
			return resourceValue;
		}

        public override string ProductUserContext
        {
            get 
            {
                throw new NotImplementedException(); 
            }
        }

        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            return 1;
        }

        public int FDoIdle(uint grfidlef)
        {
            if (null != libraryManager)
            {
                libraryManager.OnIdle();
            }
            return 0;
        }

        public int FPreTranslateMessage(MSG[] pMsg)
        {
            return 0;
        }

        public int FQueryTerminate(int fPromptUser)
        {
            return 1;
        }

        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
        {
            return IntPtr.Zero;
        }

        public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved)
        {
        }

        public void OnAppActivate(int fActive, uint dwOtherThreadID)
        {
        }

        public void OnEnterState(uint uStateID, int fEnter)
        {
        }

        public void OnLoseActivation()
        {
        }

        public void Terminate()
        {
        }
    }
}
