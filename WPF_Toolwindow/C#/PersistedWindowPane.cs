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
using Microsoft.VisualStudio.Shell.Interop;
using MsVsShell = Microsoft.VisualStudio.Shell;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
	/// <summary>
	/// This PersistedWindowPane demonstrates the following features:
	///	 - Hosting a user control in a tool window
	///	 - Persistence (visible when VS starts based on state when VS closed)
	///	 - Tool window Toolbar
	///	 - Selection tracking (content of the Properties window is based on 
	///	   the selection in that window)
	/// 
	/// Tool windows are composed of a frame (provided by Visual Studio) and a
	/// pane (provided by the package implementer). The frame implements
	/// IVsWindowFrame while the pane implements IVsWindowPane.
	/// 
	/// PersistedWindowPane inherits the IVsWindowPane implementation from its
	/// base class (ToolWindowPane). PersistedWindowPane will host a .NET
	/// UserControl (PersistedWindowControl). The Package base class will
	/// get the user control by asking for the Window property on this class.
	/// </summary>
	[Guid("0A6F8EDC-5DDB-4aaa-A6B3-2AC1E319693E")]
	class PersistedWindowPane : MsVsShell.ToolWindowPane
	{
		// Control that will be hosted in the tool window
		private PersistedWindowWPFControl control = null;

		/// <summary>
		/// Constructor for ToolWindowPane.
		/// Initialization that depends on the package or that requires access
		/// to VS services should be done in OnToolWindowCreated.
		/// </summary>
		public PersistedWindowPane()
			: base(null)
		{
            // Set the image that will appear on the tab of the window frame when docked with another window.
            // KnownMonikers is a set of image monkiers that are globablly recognized by VS. These images can be
            // used in any project without needing to include the source image.
            BitmapImageMoniker = Microsoft.VisualStudio.Imaging.KnownMonikers.Search;

			// Add the toolbar by specifying the Guid/MenuID pair corresponding to
			// the toolbar definition in the vsct file.
			ToolBar = new CommandID(GuidsList.guidClientCmdSet, PkgCmdId.IDM_MyToolbar);
			// Specify that we want the toolbar at the top of the window
			ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;

			// Creating the user control that will be displayed in the window - change this to content
            control = new PersistedWindowWPFControl();

            Content = control;
		}
        

		/// <summary>
		/// This is called after our control has been created and sited.
		/// This is a good place to initialize the control with data gathered
		/// from Visual Studio services.
		/// </summary>
		public override void OnToolWindowCreated()
		{
			base.OnToolWindowCreated();

			PackageToolWindow package = (PackageToolWindow)this.Package;

			// Set the text that will appear in the title bar of the tool window.
			// Note that because we need access to the package for localization,
			// we have to wait to do this here. If we used a constant string,
			// we could do this in the consturctor.
			this.Caption = package.GetResourceString("@100");

			// Add the handler for our toolbar button
			CommandID id = new CommandID(GuidsList.guidClientCmdSet, PkgCmdId.cmdidRefreshWindowsList);
			MsVsShell.OleMenuCommand command = DefineCommandHandler(new EventHandler(this.RefreshList), id);

			// Get the selection tracking service and pass it to the control so that it can push the
			// active selection. Only needed if you want to display something in the Properties window.
			// Note that this service is only available for windows (not in the global service provider)
			// Additionally, each window has its own (so you should not be sharing one between multiple windows)
			control.TrackSelection = (ITrackSelection)this.GetService(typeof(STrackSelection));
            
            // Ensure the control's handle has been created; otherwise, BeginInvoke cannot be called.
            // Note that during runtime this should have no effect when running inside Visual Studio,
            // as the control's handle should already be created, but unit tests can end up calling
            // this method without the control being created.
            control.InitializeComponent();

            // Delay initialization of the list until other tool windows have also had a chance to be
            // initialized
            control.Dispatcher.BeginInvoke((Action)delegate
            {
                // Populate the list view
                this.RefreshList(this, null);
            });
		}

		public override void OnToolBarAdded()
		{
			base.OnToolBarAdded();

			// In general it is not useful to override this method,
			// but it is useful when the tool window hosts a toolbar
			// with a drop-down (combo box) that needs to be initialized.
			// If that were the case, the initalization would happen here.
		}

		/// <summary>
		/// This method is called to refresh the list of items.
		/// This is the handler for the Refresh button on the toolbar
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		private void RefreshList(object sender, EventArgs arguments)
		{
			// Update the content of the control
			control.RefreshData();
		}

		/// <summary>
		/// Define a command handler.
		/// When the user presses the button corresponding to the CommandID,
		/// then the EventHandler will be called.
		/// </summary>
		/// <param name="id">The CommandID (Guid/ID pair) as defined in the .vsct file</param>
		/// <param name="handler">Method that should be called to implement the command</param>
		/// <returns>The menu command. This can be used to set parameter such as the default visibility once the package is loaded</returns>
		private MsVsShell.OleMenuCommand DefineCommandHandler(EventHandler handler, CommandID id)
		{
			// First add it to the package. This is to keep the visibility
			// of the command on the toolbar constant when the tool window does
			// not have focus. In addition, it creates the command object for us.
			PackageToolWindow package = (PackageToolWindow)this.Package;
            MsVsShell.OleMenuCommand command = package.DefineCommandHandler(handler, id);
			// Verify that the command was added
			if (command == null)
				return command;

			// Get the OleCommandService object provided by the base window pane class; this object is the one
			// responsible for handling the collection of commands implemented by the package.
			MsVsShell.OleMenuCommandService menuService = GetService(typeof(IMenuCommandService)) as MsVsShell.OleMenuCommandService;
			
			if (null != menuService)
			{
				// Add the command handler
				menuService.AddCommand(command);
			}
			return command;
		}
	}
}
