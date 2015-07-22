/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace Microsoft.Samples.VisualStudio.MenuCommands
{
    /// <summary>
    /// This is the class that implements the package. This is the class that Visual Studio will create
    /// when one of the commands will be selected by the user, and so it can be considered the main
    /// entry point for the integration with the IDE.
    /// Notice that this implementation derives from Microsoft.VisualStudio.Shell.Package that is the
    /// basic implementation of a package provided by the Managed Package Framework (MPF).
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true)]

    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidsList.guidMenuAndCommandsPkg_string)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ComVisible(true)]
    public sealed class MenuCommandsPackage : Package
    {
        #region Member Variables
        private OleMenuCommand dynamicVisibilityCommand1;
        private OleMenuCommand dynamicVisibilityCommand2;
        #endregion

        /// <summary>
        /// Default constructor of the package. This is the constructor that will be used by VS
        /// to create an instance of your package. Inside the constructor you should do only the
        /// more basic initializazion like setting the initial value for some member variable. But
        /// you should never try to use any VS service because this object is not part of VS
        /// environment yet; you should wait and perform this kind of initialization inside the
        /// Initialize method.
        /// </summary>
        public MenuCommandsPackage()
        {
        }

        /// <summary>
        /// Initialization of the package; this is the place where you can put all the initialization
        /// code that relies on services provided by Visual Studio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Now get the OleCommandService object provided by the MPF; this object is the one
            // responsible for handling the collection of commands implemented by the package.
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Now create one object derived from MenuCommand for each command defined in
                // the VSCT file and add it to the command service.

                // For each command we have to define its id that is a unique Guid/integer pair.
                CommandID id = new CommandID(GuidsList.guidMenuAndCommandsCmdSet, PkgCmdIDList.cmdidMyCommand);
                // Now create the OleMenuCommand object for this command. The EventHandler object is the
                // function that will be called when the user will select the command.
                OleMenuCommand command = new OleMenuCommand(new EventHandler(MenuCommandCallback), id);
                // Add the command to the command service.
                mcs.AddCommand(command);

                // Create the MenuCommand object for the command placed in the main toolbar.
                id = new CommandID(GuidsList.guidMenuAndCommandsCmdSet, PkgCmdIDList.cmdidMyGraph);
                command = new OleMenuCommand(new EventHandler(GraphCommandCallback), id);
                mcs.AddCommand(command);

                // Create the MenuCommand object for the command placed in our toolbar.
                id = new CommandID(GuidsList.guidMenuAndCommandsCmdSet, PkgCmdIDList.cmdidMyZoom);
                command = new OleMenuCommand(new EventHandler(ZoomCommandCallback), id);
                mcs.AddCommand(command);

                // Create the DynamicMenuCommand object for the command defined with the TextChanges
                // flag.
                id = new CommandID(GuidsList.guidMenuAndCommandsCmdSet, PkgCmdIDList.cmdidDynamicTxt);
                command = new DynamicTextCommand(id, VSPackage.ResourceManager.GetString("DynamicTextBaseText"));
                mcs.AddCommand(command);

                // Now create two OleMenuCommand objects for the two commands with dynamic visibility
                id = new CommandID(GuidsList.guidMenuAndCommandsCmdSet, PkgCmdIDList.cmdidDynVisibility1);
                dynamicVisibilityCommand1 = new OleMenuCommand(new EventHandler(DynamicVisibilityCallback), id);
                mcs.AddCommand(dynamicVisibilityCommand1);

                id = new CommandID(GuidsList.guidMenuAndCommandsCmdSet, PkgCmdIDList.cmdidDynVisibility2);
                dynamicVisibilityCommand2 = new OleMenuCommand(new EventHandler(DynamicVisibilityCallback), id);

                // This command is the one that is invisible by default, so we have to set its visble
                // property to false because the default value of this property for every object derived
                // from MenuCommand is true.
                dynamicVisibilityCommand2.Visible = false;
                mcs.AddCommand(dynamicVisibilityCommand2);
            }
        }

        #region Commands Actions
        /// <summary>
        /// This function prints text on the debug ouput and on the generic pane of the 
        /// Output window.
        /// </summary>
        /// <param name="text"></param>
        private void OutputCommandString(string text)
        {
            // Build the string to write on the debugger and Output window.
            StringBuilder outputText = new StringBuilder();
            outputText.Append(" ================================================\n");
            outputText.AppendFormat("  MenuAndCommands: {0}\n", text);
            outputText.Append(" ================================================\n\n");

            IVsOutputWindowPane windowPane = (IVsOutputWindowPane)GetService(typeof(SVsGeneralOutputWindowPane));
            if (null == windowPane)
            {
                Debug.WriteLine("Failed to get a reference to the Output window General pane");
                return;
            }
            if (Microsoft.VisualStudio.ErrorHandler.Failed(windowPane.OutputString(outputText.ToString())))
            {
                Debug.WriteLine("Failed to write on the Output window");
            }
        }

        /// <summary>
        /// Event handler called when the user selects the Sample command.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Microsoft.Samples.VisualStudio.MenuCommands.MenuCommandsPackage.OutputCommandString(System.String)")]
        private void MenuCommandCallback(object caller, EventArgs args)
        {
            OutputCommandString("Sample Command Callback.");
        }

        /// <summary>
        /// Event handler called when the user selects the Graph command.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Microsoft.Samples.VisualStudio.MenuCommands.MenuCommandsPackage.OutputCommandString(System.String)")]
        private void GraphCommandCallback(object caller, EventArgs args)
        {
            OutputCommandString("Graph Command Callback.");
        }

        /// <summary>
        /// Event handler called when the user selects the Zoom command.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Microsoft.Samples.VisualStudio.MenuCommands.MenuCommandsPackage.OutputCommandString(System.String)")]
        private void ZoomCommandCallback(object caller, EventArgs args)
        {
            OutputCommandString("Zoom Command Callback.");
        }

        /// <summary>
        /// Event handler called when the user selects one of the two menus with
        /// dynamic visibility.
        /// </summary>
        private void DynamicVisibilityCallback(object caller, EventArgs args)
        {
            // This callback is supposed to be called only from the two menus with dynamic visibility
            // defined inside this package, so first we have to verify that the caller is correct.

            // Check that the type of the caller is the expected one.
            OleMenuCommand command = caller as OleMenuCommand;
            if (null == command)
                return;

            // Now check the command set.
            if (command.CommandID.Guid != GuidsList.guidMenuAndCommandsCmdSet)
                return;

            // This is one of our commands. Now what we want to do is to switch the visibility status
            // of the two menus with dynamic visibility, so that if the user clicks on one, then this 
            // will make it invisible and the other one visible.
            if (command.CommandID.ID == PkgCmdIDList.cmdidDynVisibility1)
            {
                // The user clicked on the first one; make it invisible and show the second one.
                dynamicVisibilityCommand1.Visible = false;
                dynamicVisibilityCommand2.Visible = true;
            }
            else if (command.CommandID.ID == PkgCmdIDList.cmdidDynVisibility2)
            {
                // The user clicked on the second one; make it invisible and show the first one.
                dynamicVisibilityCommand2.Visible = false;
                dynamicVisibilityCommand1.Visible = true;
            }
        }
        #endregion
    }
}
