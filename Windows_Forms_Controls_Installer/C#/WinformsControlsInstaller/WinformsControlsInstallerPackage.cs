/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.VisualStudio.Shell;
using System;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Samples.VisualStudio.IDE.WinformsControlsInstaller
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
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidWinformsControlsInstallerPkgString)]
    [ProvideToolboxItems(ToolboxVersion)]
    public sealed class PackageWinformsToolbox : Package
    {
        // This value, passed to the constructor of ProvideToolboxItemsAttribute to generate
        // toolbox registration for the package, must be >= 1.  Increment it if your toolbox
        // content changes (for example, you have new items to install).  After the updated
        // version of your package is installed, the toolbox will notice the updated value and
        // invoke your ToolboxUpgraded event to allow you to update your content.
        const int ToolboxVersion = 1;

        public PackageWinformsToolbox()
        {
            ToolboxInitialized += new EventHandler(PackageWinformsToolbox_ToolboxInitialized);
            ToolboxUpgraded += new EventHandler(PackageWinformsToolbox_ToolboxUpgraded);
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that relies on services provided by Visual Studio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            // TODO: add initialization code here
        }
        #endregion

        /// <summary>
        /// This method is called when the toolbox content version (the parameter to the ProvideToolboxItems
        /// attribute) changes.  This tells Visual Studio that items may have changed 
        /// and need to be reinstalled.
        /// </summary>
        private void PackageWinformsToolbox_ToolboxUpgraded(object sender, EventArgs e)
        {
            RemoveToolboxItems();
            InstallToolboxItems();
        }

        /// <summary>
        /// This method will add items to the toolbox.  It is called the first time the toolbox
        /// is used after this package has been installed.
        /// </summary>
        private void PackageWinformsToolbox_ToolboxInitialized(object sender, EventArgs e)
        {
            InstallToolboxItems();
        }

        /// <summary>
        /// Removes all the toolbox items installed by this package (those which came from this
        /// assembly).
        /// </summary>
        private void RemoveToolboxItems()
        {
            Assembly a = typeof(PackageWinformsToolbox).Assembly;

            IToolboxService tbxService = (IToolboxService)GetService(typeof(IToolboxService));

            foreach (ToolboxItem item in ToolboxService.GetToolboxItems(a, newCodeBase: null))
            {
                tbxService.RemoveToolboxItem(item);
            }
        }

        /// <summary>
        /// Installs all the toolbox items defined in this assembly.
        /// </summary>
        private void InstallToolboxItems()
        {
            // For demonstration purposes, this assembly includes toolbox items and loads them from itself.
            // It is of course possible to load toolbox items from a different assembly by either:
            // a)  loading the assembly yourself and calling ToolboxService.GetToolboxItems
            // b)  calling AssemblyName.GetAssemblyName("...") and then ToolboxService.GetToolboxItems(assemblyName)
            Assembly a = typeof(PackageWinformsToolbox).Assembly;

            IToolboxService tbxService = (IToolboxService)GetService(typeof(IToolboxService));

            foreach (ToolboxItem item in ToolboxService.GetToolboxItems(a, newCodeBase: null))
            {
                // This tab name can be whatever you would like it to be.
                tbxService.AddToolboxItem(item, "MyOwnTab");
            }
        }

    }
}
