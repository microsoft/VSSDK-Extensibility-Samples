/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.CommandTargetRGB
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid(GuidList.guidToolWindowPersistenceString)]
    public class RGBToolWindow : ToolWindowPane
    {
        private RGBControl control;
        private uint checkedCommand = PkgCmdIDList.cmdidRed;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public RGBToolWindow() :  
            base(null)
        {
            // Set the window title reading it from the resources.
            this.Caption = Resources.ToolWindowTitle;

            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 0;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            control = new RGBControl();
            base.Content = control;
        }

        /// <summary>
        /// Add buttons to the toolbar. Specify the target toolbar, which button to place, and 
        /// the corresponding function call.
        /// </summary>
        private void AddCommand(OleMenuCommandService mcs, int cmdid, EventHandler handler)
        {
            // Create the command for the tool window
            CommandID commandID    = new CommandID(GuidList.guidCommandTargetRGBCmdSet, cmdid);
            OleMenuCommand command = new OleMenuCommand(handler, commandID);
            command.BeforeQueryStatus += OnBeforeQueryStatus;

            mcs.AddCommand(command);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;

            command.Enabled = true;
            command.Visible = true;
            command.Checked = (command.CommandID.ID == checkedCommand);
        }

        private void OnRedCommand(object sender, EventArgs e)
        {
            control.Color  = RGBControlColor.Red;
            checkedCommand = PkgCmdIDList.cmdidRed;
        }

        private void OnGreenCommand(object sender, EventArgs e)
        {
            control.Color  = RGBControlColor.Green;
            checkedCommand = PkgCmdIDList.cmdidGreen;
        }

        private void OnBlueCommand(object sender, EventArgs e)
        {
            control.Color  = RGBControlColor.Blue;
            checkedCommand = PkgCmdIDList.cmdidBlue;
        }

        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            // Add our command handlers for toolbar buttons
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                AddCommand(mcs, PkgCmdIDList.cmdidRed, OnRedCommand);
                AddCommand(mcs, PkgCmdIDList.cmdidGreen, OnGreenCommand);
                AddCommand(mcs, PkgCmdIDList.cmdidBlue, OnBlueCommand);
            }

            // Add the toolbar to the window
            CreateToolBar();
        }

        private void CreateToolBar()
        {
            // Retrieve the shell UI object
            IVsUIShell4 shell4 = GetService(typeof(SVsUIShell)) as IVsUIShell4;
            if (shell4 != null)
            {
                // Create the toolbar tray
                IVsToolbarTrayHost host = null;
                if (ErrorHandler.Succeeded(shell4.CreateToolbarTray(this, out host)))
                {
                    // Add the toolbar as defined in vsct
                    host.AddToolbar(GuidList.guidCommandTargetRGBCmdSet, PkgCmdIDList.RGBToolbar);

                    IVsUIElement uiElement;
                    host.GetToolbarTray(out uiElement);

                    // Get the WPF element
                    object uiObject;
                    uiElement.GetUIObject(out uiObject);
                    IVsUIWpfElement wpfe = uiObject as IVsUIWpfElement;

                    // Retrieve and set the toolbar tray
                    object frameworkElement;
                    wpfe.GetFrameworkElement(out frameworkElement);
                    control.SetTray(frameworkElement as ToolBarTray);
                }
            }
        }
    }
}
