/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using EnvDTE;
using Microsoft.Samples.VisualStudio.CodeSweep.BuildTask;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    [InstalledProductRegistration("#100", "#102", "1.0.0.0")]
    [Guid(GuidList.guidVSPackagePkgString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideBindingPath()]
    public sealed class VSPackage : Package
    {
        private readonly IChannel _tcpChannel = new TcpChannel(Utilities.RemotingChannel);

        public VSPackage()
        {
            Factory.ServiceProvider = this;
            Factory.GetBuildManager().CreatePerUserFilesAsNecessary();
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        protected override void Initialize()
        {
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidConfig);
                OleMenuCommand menuItem = new OleMenuCommand(new EventHandler(MenuItemCallback), menuCommandID);
                menuItem.BeforeQueryStatus += new EventHandler(QueryStatus);
                mcs.AddCommand(menuItem);

                // Create the commands for the tasklist toolbar.
                menuCommandID = new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidStopScan);
                menuItem = new OleMenuCommand(new EventHandler(StopScan), menuCommandID);
                menuItem.Enabled = false;
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidRepeatLastScan);
                menuItem = new OleMenuCommand(new EventHandler(RepeatLastScan), menuCommandID);
                menuItem.Enabled = false;
                mcs.AddCommand(menuItem);
            }
            else
            {
                Debug.Fail("Failed to get IMenuCommandService service.");
            }

            Factory.GetBuildManager().IsListeningToBuildEvents = true;
            Factory.GetBuildManager().BuildStarted += new EmptyEvent(BuildManager_BuildStarted);
            Factory.GetBuildManager().BuildStopped += new EmptyEvent(BuildManager_BuildStopped);
            Factory.GetBackgroundScanner().Started += new EventHandler(BackgroundScanner_Started);
            Factory.GetBackgroundScanner().Stopped += new EventHandler(BackgroundScanner_Stopped);

            if (ChannelServices.GetChannel(_tcpChannel.ChannelName) == null)
            {
                ChannelServices.RegisterChannel(_tcpChannel, ensureSecurity: false);
            }
            
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ScannerHost),
                Utilities.GetRemotingUri(System.Diagnostics.Process.GetCurrentProcess().Id, includeLocalHostPrefix: false),
                WellKnownObjectMode.Singleton
                );
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    Factory.GetBuildManager().IsListeningToBuildEvents = false;
                    Factory.CleanupFactory();
                    ChannelServices.UnregisterChannel(_tcpChannel);

                    GC.SuppressFinalize(this);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #endregion Package Members

        void BuildManager_BuildStopped()
        {
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs == null)
            {
                Debug.Fail("Failed to get IMenuCommandService service.");
                return;
            }
            mcs.FindCommand(new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidConfig)).Enabled = true;
        }

        void BuildManager_BuildStarted()
        {
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs == null)
            {
                Debug.Fail("Failed to get IMenuCommandService service.");
                return;
            }
            mcs.FindCommand(new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidConfig)).Enabled = false;
        }

        void BackgroundScanner_Stopped(object sender, EventArgs e)
        {
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs == null)
            {
                Debug.Fail("Failed to get IMenuCommandService service.");
                return;
            }

            MenuCommand stopCommand = mcs.FindCommand(new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidStopScan));
            stopCommand.Enabled = false;
            stopCommand.Checked = false;
            mcs.FindCommand(new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidRepeatLastScan)).Enabled = true;
        }

        void BackgroundScanner_Started(object sender, EventArgs e)
        {
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs == null)
            {
                Debug.Fail("Failed to get IMenuCommandService service.");
                return;
            }

            mcs.FindCommand(new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidStopScan)).Enabled = true;
            mcs.FindCommand(new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidRepeatLastScan)).Enabled = false;
        }

        private void StopScan(object sender, EventArgs e)
        {
            Factory.GetBackgroundScanner().StopIfRunning(false);
            if (Factory.GetBackgroundScanner().IsRunning)
            {
                var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
                if (mcs == null)
                {
                    Debug.Fail("Failed to get IMenuCommandService service.");
                    return;
                }

                mcs.FindCommand(new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidStopScan)).Checked = true;
            }
        }

        private void RepeatLastScan(object sender, EventArgs e)
        {
            Factory.GetBackgroundScanner().RepeatLast();
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            Factory.GetDialog().Invoke(ProjectUtilities.GetProjectsOfCurrentSelections());
        }

        /// <summary>
        /// This function is the callback used to query the status of the Code Sweep... project menu item
        /// </summary>
        void QueryStatus(object sender, EventArgs e)
        {
            bool menuVisible = false;
            var dte = GetService(typeof(DTE)) as DTE;
            if (dte == null)
            {
                Debug.Fail("Failed to get DTE service.");
                return;
            }

            foreach (EnvDTE.Project dteProject in dte.Solution.Projects)
            {
                Guid SolutionFolder = new Guid(EnvDTE.Constants.vsProjectKindSolutionItems);
                Guid MiscellaneousFiles = new Guid(EnvDTE.Constants.vsProjectKindMisc);
                Guid currentProjectKind = new Guid(dteProject.Kind);
                if (currentProjectKind != SolutionFolder && currentProjectKind != MiscellaneousFiles)
                {
                    menuVisible = true;
                }
            }

            OleMenuCommand menuCommand = sender as OleMenuCommand;
            menuCommand.Visible = menuVisible;
        }
    }
}
