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
using System.ComponentModel.Design;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace MyCompany.RdtEventExplorer
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsWindowPane interface.
    /// </summary>
    [Guid("99cd759f-e9ab-4327-985a-040573ac417a")]
    public class RdtEventWindowPane : ToolWindowPane
    {
        // This is the user control hosted by the tool window; it is exposed to the base class 
        // using the Window property. Note that, even if this class implements IDispose, we are
        // not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
        // the object returned by the Window property.
        private RdtEventControl control;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public RdtEventWindowPane() :
            base(null)
        {
            // Set the window title reading it from the resources.
            Caption = Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            BitmapResourceID = 301;
            BitmapIndex = 1;
            
            // Add the toolbar by specifying the Guid/MenuID pair corresponding to
            // the toolbar definition in the vsct file.
            ToolBar = new CommandID(GuidsList.guidRdtEventExplorerCmdSet, PkgCmdIDList.IDM_MyToolbar);
            // Specify that we want the toolbar at the top of the window
            ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
            control = new RdtEventControl();
        }
        /// <summary>
        /// This is called after our control has been created and sited.
        /// This is a good place to initialize the control with data gathered
        /// from Visual Studio services.
        /// </summary>
        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            // Add the handler for our toolbar button
            CommandID id = new CommandID(GuidsList.guidRdtEventExplorerCmdSet, PkgCmdIDList.cmdidRefreshWindowsList);
            DefineCommandHandler(new EventHandler(RefreshGrid), id);

            id = new CommandID(GuidsList.guidRdtEventExplorerCmdSet, PkgCmdIDList.cmdidClearWindowsList);
            DefineCommandHandler(new EventHandler(ClearGrid), id);
            //            OleMenuCommand command = DefineCommandHandler(new EventHandler(this.RefreshGrid), id);
            
            // Get the selection tracking service and pass it to the control so that it can push the
            // active selection. Only needed if you want to display something in the Properties window.
            // Note that this service is only available for windows (not in the global service provider)
            // Additionally, each window has its own (so you should not be sharing one between multiple windows)
            control.TrackSelection = (ITrackSelection)GetService(typeof(STrackSelection));
        }
        /// <summary>
        /// This property returns the handle to the user control that should
        /// be hosted in the Tool Window.
        /// </summary>
        override public IWin32Window Window
        {
            get
            {
                return control;
            }
        }
        /// <summary>
        /// This method is called to refresh the list of items.
        /// This is the handler for the Refresh button on the toolbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        private void RefreshGrid(object sender, EventArgs arguments)
        {
            // Update the content of the control
            control.RefreshGrid();
        }
        /// <summary>
        /// This method is called to clear the list of items.
        /// This is the handler for the Clear button on the toolbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        private void ClearGrid(object sender, EventArgs arguments)
        {
            // Update the content of the control
            control.ClearGrid();
        }

        /// <summary>
        /// Define a command handler.
        /// When the user presses the button corresponding to the CommandID,
        /// then the EventHandler will be called.
        /// </summary>
        /// <param name="id">The CommandID (Guid/ID pair) as defined in the .vsct file</param>
        /// <param name="handler">Method that should be called to implement the command</param>
        /// <returns>The menu command. This can be used to set parameter such as the default visibility once the package is loaded</returns>
        private OleMenuCommand DefineCommandHandler(EventHandler handler, CommandID id)
        {
            // First add it to the package. This is to keep the visibility
            // of the command on the toolbar constant when the tool window does
            // not have focus. In addition, it creates the command object for us.
            RdtEventExplorerPkg package = (RdtEventExplorerPkg) Package;
            OleMenuCommand command = package.DefineCommandHandler(handler, id);
            // Verify that the command was added
            if (command == null)
                return command;

            // Get the OleCommandService object provided by the base window pane class; this object is the one
            // responsible for handling the collection of commands implemented by the package.
            OleMenuCommandService menuService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (null != menuService)
            {
                // Add the command handler
                menuService.AddCommand(command);
            }
            return command;
        }
    }
}
