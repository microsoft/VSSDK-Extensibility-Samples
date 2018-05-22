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
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;

using MsVsShell = Microsoft.VisualStudio.Shell;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.BasicSccProvider
{
    /// <summary>
    /// BasicSccProvider class. 
    /// </summary>
    // Declare that resources for the package are to be found in the managed assembly resources, and not in a satellite dll
    [MsVsShell.PackageRegistration(UseManagedResourcesOnly = true)]
    // Register the resource ID of the CTMENU section (generated from compiling the VSCT file), so the IDE will know how to merge this package's menus with the rest of the IDE when "devenv /setup" is run
    // The menu resource ID needs to match the ResourceName number defined in the csproj project file in the VSCTCompile section
    // Everytime the version number changes VS will automatically update the menus on startup; if the version doesn't change, you will need to run manually "devenv /setup /rootsuffix:Exp" to see VSCT changes reflected in IDE
    [MsVsShell.ProvideMenuResource("Menus.ctmenu", 1)]
    // Register the product to be listed in About box
    [MsVsShell.InstalledProductRegistration("#100", "#101", "1.0", IconResourceID = CommandId.iiconProductIcon) ]
    // Register a sample options page visible as Tools/Options/SourceControl/SampleOptionsPage when the provider is active
    [MsVsShell.ProvideOptionPageAttribute(typeof(SccProviderOptions), "Source Control", "Sample Options Page Basic Provider", 106, 107, false)]
    [ProvideToolsOptionsPageVisibility("Source Control", "Sample Options Page Basic Provider", "ADC98052-0000-41D1-A6C3-704E6C1A3DE2")]
    // Register a sample tool window visible only when the provider is active
    [MsVsShell.ProvideToolWindow(typeof(SccProviderToolWindow))]
    [MsVsShell.ProvideToolWindowVisibility(typeof(SccProviderToolWindow), "ADC98052-0000-41D1-A6C3-704E6C1A3DE2")]
    // Register the source control provider's service (implementing IVsScciProvider interface)
    [MsVsShell.ProvideService(typeof(SccProviderService), ServiceName = "Source Control Sample Basic Provider Service")]
    // Register the source control provider to be visible in Tools/Options/SourceControl/Plugin dropdown selector
    [ProvideSourceControlProvider("Managed Source Control Sample Basic Provider", "#100")]
    // Pre-load the package when the command UI context is asserted (the provider will be automatically loaded after restarting the shell if it was active last time the shell was shutdown)
    [MsVsShell.ProvideAutoLoad("ADC98052-0000-41D1-A6C3-704E6C1A3DE2")]
    // Declare the package guid
    [Guid("ADC98052-2000-41D1-A6C3-704E6C1A3DE2")]
    public class BasicSccProvider : MsVsShell.Package
    {
        private SccProviderService sccService = null;

        public BasicSccProvider()
        {}
        
        #region Package Members

        /// <summary>
        /// Called when the package is loaded, performs package initialization tasks such as registering the source control provider
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Proffer the source control service implemented by the provider
            sccService = new SccProviderService(this);
            ((IServiceContainer)this).AddService(typeof(SccProviderService), sccService, true);

            // Add our command handlers for menu (commands must exist in the .vsct file)
            MsVsShell.OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as MsVsShell.OleMenuCommandService;
            if (mcs != null)
            {
                CommandID cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdSccCommand);
                MenuCommand menuCmd = new MenuCommand(new EventHandler(OnSccCommand), cmd);
                mcs.AddCommand(menuCmd);

                // ToolWindow Command
                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdViewToolWindow);
                menuCmd = new MenuCommand(new EventHandler(ViewToolWindow), cmd);
                mcs.AddCommand(menuCmd);

                // ToolWindow's ToolBar Command
                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdToolWindowToolbarCommand);
                menuCmd = new MenuCommand(new EventHandler(ToolWindowToolbarCommand), cmd);
                mcs.AddCommand(menuCmd);
            }

            // Register the provider with the source control manager
            // If the package is to become active, this will also callback on OnActiveStateChange and the menu commands will be enabled
            IVsRegisterScciProvider rscp = (IVsRegisterScciProvider)GetService(typeof(IVsRegisterScciProvider));
            rscp.RegisterSourceControlProvider(GuidList.guidSccProvider);
        }

        /// <summary>
        /// Releases the resources used by the Package object
        /// </summary>
        /// <param name="disposing">true if the object is being disposed, false if it is being finalized</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        /// Called whenever Tools > SccCommand button is pressed
        /// </summary>
        private void OnSccCommand(object sender, EventArgs e)
        {
            // Toggle the checked state of this command
            MenuCommand thisCommand = sender as MenuCommand;
            if (thisCommand != null)
            {
                thisCommand.Checked = !thisCommand.Checked;
            }
        }

        /// <summary>
        /// This function can be used to bring back the provider's toolwindow if it was previously closed.
        /// (Called whenever View > Source control provider toolwindow button is pressed)
        /// </summary>
        private void ViewToolWindow(object sender, EventArgs e)
        {
            ShowSccProviderToolWindow();
        }
        
        /// <summary>
        /// This function gets called whenever the button in the SCC Toolwindow gets pressed. Locates the appropriate
        /// ToolWindow and calls the requested function
        /// </summary>
        private void ToolWindowToolbarCommand(object sender, EventArgs e)
        {
            SccProviderToolWindow window = (SccProviderToolWindow)FindToolWindow(typeof(SccProviderToolWindow), 0, true);

            if (window != null)
            {
                window.ToolWindowToolbarCommand();
            }
        }

        /// <summary>
        /// This function is called by the IVsSccProvider service implementation when the active state of the provider changes
        /// The package needs to show or hide the scc-specific commands 
        /// </summary>
        public virtual void OnActiveStateChange()
        {
            MsVsShell.OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as MsVsShell.OleMenuCommandService;
            if (mcs != null)
            {
                CommandID cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdSccCommand);
                MenuCommand menuCmd = mcs.FindCommand(cmd);
                menuCmd.Supported = true;
                menuCmd.Enabled = sccService.Active;
                menuCmd.Visible = sccService.Active;

                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdViewToolWindow);
                menuCmd = mcs.FindCommand(cmd);
                menuCmd.Supported = true;
                menuCmd.Enabled = sccService.Active;
                menuCmd.Visible = sccService.Active;

                cmd = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.icmdToolWindowToolbarCommand);
                menuCmd = mcs.FindCommand(cmd);
                menuCmd.Supported = true;
                menuCmd.Enabled = sccService.Active;
                menuCmd.Visible = sccService.Active;
            }

            ShowSccProviderToolWindow();
        }

        /// <summary>
        /// Display the SCC Provider Toolwindow if it is not already visible
        /// </summary>
        private void ShowSccProviderToolWindow()
        {
            IVsWindowFrame windowFrame = null;

            try
            {
                // This function is called when the package is auto-loaded (as result of our command UI context 
                // guidSccProvider being set active). This can happen on startup, if this scc provider was active 
                // last time the shell was started.
                // However, at that time we cannot create the toolwindow because the shell is not fully initialized
                // and the window profile is not yet loaded. We need to protect
                MsVsShell.ToolWindowPane window = FindToolWindow(typeof(SccProviderToolWindow), 0, true);
                if (window != null && window.Frame != null)
                {
                    windowFrame = (IVsWindowFrame)window.Frame;
                }
            }
            catch (COMException e)
            {
                if (e.ErrorCode != VSConstants.E_UNEXPECTED)
                    throw;
            }

            if (windowFrame == null)
            {
                return;
            }

            if (sccService.Active)
            {
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
            else
            {
                ErrorHandler.ThrowOnFailure(windowFrame.Hide());
            }
        }
    }
}