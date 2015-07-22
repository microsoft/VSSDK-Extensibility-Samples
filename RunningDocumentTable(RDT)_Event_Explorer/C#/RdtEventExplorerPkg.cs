/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

// VsPkg.cs : Implementation of RdtExplorer

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace MyCompany.RdtEventExplorer
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
    // This attribute tells the registration utility (regpkg.exe) that this class needs
    // to be registered as package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // A Visual Studio component can be registered under different regitry roots; for instance
    // when you debug your package you want to register it in the experimental instance. This
    // attribute specifies the registry root to use if no one is provided to regpkg.exe with
    // the /root switch.
    
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#100", "#102", "1.0", IconResourceID = 400)]
    
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource(1000, 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(RdtEventWindowPane))]
    // This attribute registers a Tools/Options grid dialog page to set display options.
    [ProvideOptionPage(typeof(RdtEventOptionsDialog), "RDT Event Explorer", "Explorer Options", 0, 0, true)]
    [Guid(GuidsList.guidRdtEventExplorerPkgString)]
    public sealed class RdtEventExplorerPkg : Package
    {
        // Cache the Menu Command Service since we will use it multiple times
        private OleMenuCommandService menuService;

        const int bitmapResourceID = 300;
 
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public RdtEventExplorerPkg()
        {
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(RdtEventWindowPane), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new COMException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
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

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidsList.guidRdtEventExplorerCmdSet, (int)PkgCmdIDList.cmdidMyTool);
                MenuCommand menuToolWin = new MenuCommand( new EventHandler(ShowToolWindow), toolwndCommandID);
                mcs.AddCommand( menuToolWin );
            }

            //// Add a global service to provide a single set of options.
            //((IServiceContainer)this).AddService(typeof(SMyOptionsService), this, true);

            //// Now that we have the service, we can use it to initialize options.
            //// At this point options is null, so we will initialize it from the registry.
            //GetRdtEventOptions();
        }
        
        /// <summary>
        /// Define a command handler.
        /// When the user press the button corresponding to the CommandID
        /// the EventHandler will be called.
        /// </summary>
        /// <param name="id">The CommandID (Guid/ID pair) as defined in the .vsct file</param>
        /// <param name="handler">Method that should be called to implement the command</param>
        /// <returns>The menu command. This can be used to set parameter such as the default visibility once the package is loaded</returns>
        internal OleMenuCommand DefineCommandHandler(EventHandler handler, CommandID id)
        {
            // if the package is zombied, we don't want to add commands
            if (Zombied)
                return null;

            // Make sure we have the service
            if (menuService == null)
            {
                // Get the OleCommandService object provided by the MPF; this object is the one
                // responsible for handling the collection of commands implemented by the package.
                menuService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            }
            OleMenuCommand command = null;
            if (null != menuService)
            {
                // Add the command handler
                command = new OleMenuCommand(handler, id);
                menuService.AddCommand(command);
            }
            return command;
        }
        #endregion
    }
}