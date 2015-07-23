/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Samples.VisualStudio.CodeSweep.VSPackage.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    class BuildManager : IBuildManager
    {
        public const string RunWithBuildFlag = "RunCodeSweepAfterBuild";
        const string VsProcIdProperty = "HostIdeProcId";

        /// <summary>
        /// Creates a new build manager object.
        /// </summary>
        /// <param name="provider">The service provider that will be used to get VS services.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>serviceProvider</c> is null.</exception>
        public BuildManager(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            _serviceProvider = provider;
        }

        #region IBuildManager Members

        public event EmptyEvent BuildStarted;
        public event EmptyEvent BuildStopped;

        /// <summary>
        /// Gets or sets whether this object is subscribed to the build events fired by the VS IDE.
        /// </summary>
        /// <remarks>
        /// If this object is subscribed to build events, it will clear the CodeSweep task list and
        /// set the host object for all ScannerTask tasks in all projects when the build begins.
        /// </remarks>
        public bool IsListeningToBuildEvents
        {
            get
            {
                return _listening;
            }
            set
            {
                if (value != _listening)
                {
                    if (value)
                    {
                        GetBuildEvents().OnBuildBegin += buildEvents_OnBuildBegin;
                        GetBuildEvents().OnBuildDone += buildEvents_OnBuildDone;
                    }
                    else
                    {
                        GetBuildEvents().OnBuildBegin -= buildEvents_OnBuildBegin;
                        GetBuildEvents().OnBuildDone -= buildEvents_OnBuildDone;
                    }
                    _listening = value;
                }
            }
        }

        /// <summary>
        /// Gets the CodeSweep build task for the given project, optionally creating it if it does not exist.
        /// </summary>
        /// <param name="project">The project from which the task will be retrieved.</param>
        /// <param name="createIfNecessary">If true, the task will be created if it does not exist.</param>
        /// <returns>The task object retrieved, or null if no task was found and <c>createIfNecessary</c> is false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>project</c> is null.</exception>
        /// <remarks>
        /// If the task is created, a <c>UsingTask</c> entry is also inserted into the project to
        /// specify the location of the dll.
        /// The task is created in the "AfterBuild" target of the project file.
        /// The task is created with the following properties:
        ///     Condition = "'$(RunCodeSweepAfterBuild)' == 'true'"
        ///     ContinueOnError = false
        /// and the following parameters:
        ///     FilesToScan = all item groups in project except "Reference", formatted as "@(group1);@(group2);..."
        ///     TermTables = [user's app data folder]\CodeSweep\sample_term_table.xml
        ///     Project = "$(MSBuildProjectFullPath)"
        /// </remarks>
        public ProjectTaskElement GetBuildTask(IVsProject project, bool createIfNecessary)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            if (createIfNecessary)
            {
                CreateTaskIfNecessary(project);
            }

            Project msbuildProject = MSBuildProjectFromIVsProject(project);
            return GetNamedTask(msbuildProject.Xml.Targets.FirstOrDefault(element => element.Name == "AfterBuild"), "ScannerTask");
        }

        /// <summary>
        /// Enumerates all items in the project except those in the "Reference" group.
        /// </summary>
        /// <param name="project">The project from which to retrieve the items.</param>
        /// <returns>A list of item "Include" values.  For items that specify files, these will be the file names.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>project</c> is null.</exception>
        public IEnumerable<string> AllItemsInProject(IVsProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            string projectDir = Path.GetDirectoryName(ProjectUtilities.GetProjectFilePath(project));
            IVsHierarchy hierarchy = project as IVsHierarchy;

            return
                ChildrenOf(hierarchy, VSConstants.VSITEMID.Root)
                .Select(
                    id =>
                    {
                        string name = null;
                        project.GetMkDocument((uint)id, out name);
                        if (name != null && name.Length > 0 && !Path.IsPathRooted(name))
                        {
                            name = Utilities.AbsolutePathFromRelative(name, projectDir);
                        }
                        return name;
                    })
                .Where(File.Exists);
        }

        /// <summary>
        /// Sets a property in the given project, overriding the existing value if it exists.
        /// </summary>
        /// <param name="project">The project in which the property will be set.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>project</c>, <c>name</c>, or <c>value</c> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <c>name</c> is empty or contains invalid characters.</exception>
        public void SetProperty(IVsProject project, string name, string value)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Resources.EmptyProperty, "name");
            }

            Project msbuildProject = MSBuildProjectFromIVsProject(project);
            ProjectPropertyElement property = FindProperty(msbuildProject.Xml, name);
            if (property == null)
            {
                ProjectPropertyGroupElement group = msbuildProject.Xml.AddPropertyGroup();
                group.AddProperty(name, value);
            }
            else
            {
                property.Value = value;
            }
        }

        /// <summary>
        /// Gets a property from the given project.
        /// </summary>
        /// <param name="project">The project from which the property will be retrieved.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>The value of the specified property, or null if it does not exist.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>project</c> or <c>name</c> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <c>name</c> is empty.</exception>
        public string GetProperty(IVsProject project, string name)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Resources.EmptyProperty, "name");
            }

            ProjectRootElement msbuildProject = MSBuildProjectFromIVsProject(project).Xml;
            ProjectPropertyElement property = FindProperty(msbuildProject, name);

            if (property == null)
            {
                return null;
            }
            else
            {
                return property.Value;
            }
        }

        /// <summary>
        /// Creates the per-user files for the current user if they have not yet been created.
        /// </summary>
        public void CreatePerUserFilesAsNecessary()
        {
            CreateDefaultTermTableIfNecessary();
            CreateInclusionListIfNecessary();
        }

        #endregion IBuildManager Members

        #region Private Members

        IServiceProvider _serviceProvider;
        bool _listening = false;
        EnvDTE.BuildEvents _buildEvents;

        private static void CreateDefaultTermTableIfNecessary()
        {
            if (!File.Exists(Globals.DefaultTermTablePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Globals.DefaultTermTablePath));
                File.Copy(Globals.DefaultTermTableInstallPath, Globals.DefaultTermTablePath);
            }
        }

        private static void CreateInclusionListIfNecessary()
        {
            if (!File.Exists(Globals.AllowedExtensionsPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Globals.AllowedExtensionsPath));
                File.Copy(Globals.AllowedExtensionsInstallPath, Globals.AllowedExtensionsPath);
            }
        }

        private void SetHostProcId()
        {
            foreach (IVsProject project in ProjectUtilities.LoadedProjects)
            {
                ProjectTaskElement task = GetBuildTask(project, createIfNecessary: false);
                if (task != null)
                {
                    task.SetParameter(VsProcIdProperty, Process.GetCurrentProcess().Id.ToString());
                }
            }
        }

        void ClearHostProcId()
        {
            foreach (IVsProject project in ProjectUtilities.LoadedProjects)
            {
                ProjectTaskElement task = GetBuildTask(project, createIfNecessary: false);
                if (task != null)
                {
                    task.RemoveParameter(VsProcIdProperty);
                }
            }
        }

        private static Project MSBuildProjectFromIVsProject(IVsProject project)
        {
            return ProjectCollection.GlobalProjectCollection.GetLoadedProjects(ProjectUtilities.GetProjectFilePath(project)).FirstOrDefault();
        }

        private static ProjectTaskElement GetNamedTask(ProjectTargetElement target, string taskName)
        {
            if (target != null)
            {
                foreach (ProjectTaskElement task in target.Tasks)
                {
                    if (task.Name == taskName)
                    {
                        return task;
                    }
                }
            }

            return null;
        }

        private static void CreateTaskIfNecessary(IVsProject project)
        {
            Project msbuildProject = MSBuildProjectFromIVsProject(project);
            ProjectTargetElement target = msbuildProject.Xml.Targets.FirstOrDefault(element => element.Name == "AfterBuild");
            if (target == null)
            {
                target = msbuildProject.Xml.AddTarget("AfterBuild");
            }

            string importFolder = Path.GetDirectoryName(typeof(CodeSweep.BuildTask.ScannerTask).Module.FullyQualifiedName);
            string importPath = Utilities.EncodeProgramFilesVar(importFolder) + "\\CodeSweep.targets";
            string installedCondition = "Exists('" + importPath + "')";

            if (GetNamedTask(target, "ScannerTask") == null)
            {
                string projectFolder = Path.GetDirectoryName(ProjectUtilities.GetProjectFilePath(project));
                ProjectTaskElement newTask = target.AddTask("ScannerTask");
                newTask.Condition = installedCondition + " and '$(" + RunWithBuildFlag + ")' == 'true'";
                newTask.ContinueOnError = "false";
                newTask.SetParameter("FilesToScan", CodeSweep.Utilities.Concatenate(AllItemGroupsInProject(msbuildProject.Xml), ";"));
                newTask.SetParameter("TermTables", Utilities.RelativePathFromAbsolute(Globals.DefaultTermTablePath, Path.GetDirectoryName(msbuildProject.FullPath)));
                newTask.SetParameter("Project", "$(MSBuildProjectFullPath)");
            }

            bool found = false;

            foreach (ProjectImportElement import in msbuildProject.Xml.Imports)
            {
                if (import.Project == importPath)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                ProjectImportElement importElement = msbuildProject.Xml.AddImport(importPath);
                importElement.Condition = installedCondition;
            }
        }

        private static ProjectPropertyElement FindProperty(ProjectRootElement msbuildProject, string name)
        {
            foreach (ProjectPropertyGroupElement group in msbuildProject.PropertyGroups)
            {
                foreach (ProjectPropertyElement property in group.Properties)
                {
                    if (property.Name == name)
                    {
                        return property;
                    }
                }
            }
            return null;
        }

        private static IEnumerable<string> AllItemGroupsInProject(ProjectRootElement project)
        {
            List<string> names = new List<string>();

            foreach (ProjectItemGroupElement group in project.ItemGroups)
            {
                foreach (ProjectItemElement item in group.Items)
                {
                    if (item.ItemType != "Reference")
                    {
                        string groupName = "@(" + item.ItemType + ")";
                        if (!names.Contains(groupName))
                        {
                            names.Add(groupName);
                        }
                    }
                }
            }

            return names;
        }

        private void buildEvents_OnBuildBegin(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
        {
            if (Action == EnvDTE.vsBuildAction.vsBuildActionBuild || Action == EnvDTE.vsBuildAction.vsBuildActionRebuildAll)
            {
                Factory.GetBackgroundScanner().StopIfRunning(true);
                Factory.GetTaskProvider().Clear();
                SetHostProcId();

                var handler = BuildStarted;
                if (handler != null)
                {
                    handler();
                }
            }
        }

        void buildEvents_OnBuildDone(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
        {
            ClearHostProcId();

            var handler = BuildStopped;
            if (handler != null)
            {
                handler();
            }
        }

        private IEnumerable<string> ProjectNames
        {
            get
            {
                var solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
                if (solution == null)
                {
                    Debug.Fail("Failed to get SVsSolution service.");
                    return Enumerable.Empty<string>();
                }

                uint projectCount = 0;
                int hr = solution.GetProjectFilesInSolution((uint)__VSGETPROJFILESFLAGS.GPFF_SKIPUNLOADEDPROJECTS, 0, null, out projectCount);
                Debug.Assert(hr == VSConstants.S_OK, "GetProjectFilesInSolution failed.");

                string[] projectNames = new string[projectCount];
                hr = solution.GetProjectFilesInSolution((uint)__VSGETPROJFILESFLAGS.GPFF_SKIPUNLOADEDPROJECTS, projectCount, projectNames, out projectCount);
                Debug.Assert(hr == VSConstants.S_OK, "GetProjectFilesInSolution failed.");

                return projectNames;
            }
        }

        private EnvDTE.BuildEvents GetBuildEvents()
        {
            if (_buildEvents == null)
            {
                var dte = _serviceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
                if (dte == null)
                {
                    Debug.Fail("Failed to get DTE service.");
                    return null;
                }
                _buildEvents = dte.Events.BuildEvents;
            }
            return _buildEvents;
        }

        private List<VSConstants.VSITEMID> ChildrenOf(IVsHierarchy hierarchy, VSConstants.VSITEMID rootID)
        {
            var result = new List<VSConstants.VSITEMID>();

            for (VSConstants.VSITEMID itemID = FirstChild(hierarchy, rootID); itemID != VSConstants.VSITEMID.Nil; itemID = NextSibling(hierarchy, itemID))
            {
                result.Add(itemID);
                result.AddRange(ChildrenOf(hierarchy, itemID));
            }

            return result;
        }

        private static VSConstants.VSITEMID FirstChild(IVsHierarchy hierarchy, VSConstants.VSITEMID rootID)
        {
            object childIDObj = null;
            hierarchy.GetProperty((uint)rootID, (int)__VSHPROPID.VSHPROPID_FirstChild, out childIDObj);
            if (childIDObj != null)
            {
                return (VSConstants.VSITEMID)(int)childIDObj;
            }

            return VSConstants.VSITEMID.Nil;
        }

        private static VSConstants.VSITEMID NextSibling(IVsHierarchy hierarchy, VSConstants.VSITEMID firstID)
        {
            object siblingIDObj = null;
            hierarchy.GetProperty((uint)firstID, (int)__VSHPROPID.VSHPROPID_NextSibling, out siblingIDObj);
            if (siblingIDObj != null)
            {
                return (VSConstants.VSITEMID)(int)siblingIDObj;
            }

            return VSConstants.VSITEMID.Nil;
        }

        #endregion Private Members
    }
}
