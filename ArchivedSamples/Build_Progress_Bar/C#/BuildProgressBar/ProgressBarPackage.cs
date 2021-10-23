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

namespace Microsoft.BuildProgressBar
{
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(BuildProgressToolWindow))]
    [Guid(GuidList.guidProgressBarPkgString)]
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
    public sealed class ProgressBarPackage : Package, IVsShellPropertyEvents, IVsSolutionEvents, IVsUpdateSolutionEvents2
    {
        private int visualEffectsAllowed = 0;
        private uint shellPropertyChangesCookie;
        private uint solutionEventsCookie;
        private uint updateSolutionEventsCookie;

        private IVsShell vsShell = null;
        private IVsSolution2 solution = null;
        private IVsSolutionBuildManager2 sbm = null;

        private BuildProgressToolWindow toolWindow = null;

        private double totalProjects = 0;
        private double currProject = 0;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public ProgressBarPackage()
        {
        }

        /// <summary>
        /// Override of Package.Dispose() function from Microsoft.VisualStudio.Shell
        /// </summary>
        /// <param name="disposing">
        /// Bool Value: True if the object is being disposed, false if it is being finalized.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // Unadvise all events
            if (vsShell != null && shellPropertyChangesCookie != 0)
                vsShell.UnadviseShellPropertyChanges(shellPropertyChangesCookie);

            if (sbm != null && updateSolutionEventsCookie != 0)
                sbm.UnadviseUpdateSolutionEvents(updateSolutionEventsCookie);

            if (solution != null && solutionEventsCookie != 0)
                solution.UnadviseSolutionEvents(solutionEventsCookie);
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
            toolWindow = FindToolWindow(typeof(BuildProgressToolWindow), 0, true) as BuildProgressToolWindow;
            if ((null == toolWindow) || (null == toolWindow.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)toolWindow.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidProgressBarCmdSet, (int)PkgCmdIDList.cmdidProgressBar);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);
            }

            // Get shell object
            vsShell = ServiceProvider.GlobalProvider.GetService(typeof(SVsShell)) as IVsShell;
            if (vsShell != null)
            {
                // Initialize VisualEffects values, so themes can determine if various effects are supported by the environment
                object effectsAllowed;
                if (ErrorHandler.Succeeded(vsShell.GetProperty((int)__VSSPROPID4.VSSPROPID_VisualEffectsAllowed, out effectsAllowed)))
                {
                    // VSSPROPID_VisualEffectsAllowed is a VT_I4 property, so casting to int should be safe
                    Debug.Assert(effectsAllowed is int, "VSSPROPID_VisualEffectsAllowed should be of type int");
                    visualEffectsAllowed = (int)effectsAllowed;
                }
                else
                {
                    Debug.Fail("Failed to get the VSSPROPID_VisualEffectsAllowed property value.");
                }

                // Subscribe to shell property changes to update VisualEffects values if the user modifies the settings
                vsShell.AdviseShellPropertyChanges(this, out shellPropertyChangesCookie);
            }

            // Get solution
            solution = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolution)) as IVsSolution2;
            if (solution != null)
            {
                // Get count of any currently loaded projects
                object count;
                solution.GetProperty((int)__VSPROPID.VSPROPID_ProjectCount, out count);
                totalProjects = (int)count;

                // Register for solution events
                solution.AdviseSolutionEvents(this, out solutionEventsCookie);
            }

            // Get solution build manager
            sbm = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            if (sbm != null)
            {
                sbm.AdviseUpdateSolutionEvents(this, out updateSolutionEventsCookie);
            }

            // Get tool window
            if (toolWindow == null)
            {
                toolWindow = FindToolWindow(typeof(BuildProgressToolWindow), 0, true) as BuildProgressToolWindow;
            }

            // Set initial value of EffectsEnabled in tool window
            toolWindow.EffectsEnabled = visualEffectsAllowed != 0;
        }
        #endregion

        /// <summary>
        /// Called whenever a shell property changes.
        /// </summary>
        /// <param name="propid">ID of the property that changed</param>
        /// <param name="var">New value of the property</param>
        /// <returns>
        /// Returns S_OK if the method succeeds, Returns an error code if it fails
        /// </returns>
        int IVsShellPropertyEvents.OnShellPropertyChange(int propid, object var)
        {
            // If the propid matches the value we're interested in, query the new value
            if (propid == (int)__VSSPROPID4.VSSPROPID_VisualEffectsAllowed)
            {
                visualEffectsAllowed = (int)var;
                toolWindow.EffectsEnabled = visualEffectsAllowed != 0;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Notifies listening clients that a solution has been closed
        /// </summary>
        /// <param name="pUnkReserved">Reserved for future use</param>
        /// <returns>
        /// Returns S_OK if the method succeeds, Returns an error code if it fails
        /// </returns>
        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            // Reset progress bar after closing solution
            totalProjects = 0;

            if (toolWindow != null)
            {
                toolWindow.BarText = "";
                toolWindow.Progress = 0;
            }

            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            // Track the number of open projects
            totalProjects++;

            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            // Track the number of open projects
            totalProjects--;

            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            //This method is called when a project is unloaded
            if (toolWindow != null)
            {
                toolWindow.BarText = "Unloading build.";
                toolWindow.Progress = 0;
            }
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            // This method is called when a specific project begins building.  Based on the total number of open projects, we can estimate
            // how far along in the build we are.
            currProject++;

            if (toolWindow != null)
            {
                // Update progress bar text
                object o;
                pHierProj.GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_Name, out o);
                string name = o as string;
                toolWindow.BarText = "Building " + name + "...";

                // Update bar value; estimate percentage completion
                if (totalProjects != 0)
                {
                    double value = currProject / (totalProjects * 2);
                    toolWindow.Progress = value;
                }
            }

            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            // This method is called when a specific project finishes building.  Move the progress bar value accordginly.
            if (toolWindow != null)
            {
                toolWindow.BarText = "";

                if (totalProjects != 0)
                {
                    toolWindow.Progress = (++currProject) / (totalProjects * 2);
                }
            }

            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            // This method is called when the entire solution starts to build.
            currProject = 0;
            if (toolWindow != null)
            {
                toolWindow.BarText = "Starting build.";
                toolWindow.Progress = 0;
            }

            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            // This method is called when the entire solution is done building.
            if (toolWindow != null)
            {
                if (fSucceeded != 0)
                {
                    toolWindow.BarText = "Build completed.";
                    toolWindow.Progress = 1;
                }
                else if (fCancelCommand != 0)
                {
                    toolWindow.BarText = "Build canceled.";
                }
            }

            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }
    }
}
