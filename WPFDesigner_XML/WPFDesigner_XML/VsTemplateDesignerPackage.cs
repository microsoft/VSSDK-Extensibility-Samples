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
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VsTemplateDesigner
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]

    // Register the class as a Designer View in cooperation with the Xml Editor
    [ProvideXmlEditorChooserDesignerView("VSTemplate", "vstemplate", LogicalViewID.Designer, 0x60,
        DesignerLogicalViewEditor = typeof(EditorFactory),
        Namespace = "http://schemas.microsoft.com/developer/vstemplate/2005",
        MatchExtensionAndNamespace = false)]
    // And which type of files we want to handle
    [ProvideEditorExtension(typeof(EditorFactory), EditorFactory.Extension, 0x40, NameResourceID = 106)]
    // We register that our editor supports LOGVIEWID_Designer logical view
    [ProvideEditorLogicalView(typeof(EditorFactory), LogicalViewID.Designer)]

    // We register the XML Editor ("{FA3CD31E-987B-443A-9B81-186104E8DAC1}") as an EditorFactoryNotify
    // object to handle our ".vstemplate" file extension for the following projects:
    // Microsoft Visual Basic Project
    [EditorFactoryNotifyForProject("{F184B08F-C81C-45F6-A57F-5ABD9991F28F}", EditorFactory.Extension, GuidList.guidXmlChooserEditorFactory)]
    // Microsoft Visual C# Project
    [EditorFactoryNotifyForProject("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}", EditorFactory.Extension, GuidList.guidXmlChooserEditorFactory)]

    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidVsTemplateDesignerPkgString)]
    public sealed class VsTemplateDesignerPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public VsTemplateDesignerPackage()
        {
            
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            //Create Editor Factory. Note that the base Package class will call Dispose on it.
            base.RegisterEditorFactory(new EditorFactory(this));

        }
        #endregion

    }
}
