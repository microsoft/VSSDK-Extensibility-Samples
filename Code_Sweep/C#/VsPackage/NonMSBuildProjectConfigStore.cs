/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using EnvDTE;
using Microsoft.Samples.VisualStudio.CodeSweep.VSPackage.Properties;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    class NonMSBuildProjectConfigStore : IProjectConfigurationStore
    {
        public NonMSBuildProjectConfigStore(IVsProject project, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _project = project;

            ReadConfigFromProject();

            _termTableFiles.ItemAdded += new EventHandler<ItemEventArgs<string>>(_termTableFiles_ItemAdded);
            _termTableFiles.ItemsRemoved += new EventHandler(_termTableFiles_ItemsRemoved);
            _ignoreInstances.ItemAdded += new EventHandler<ItemEventArgs<BuildTask.IIgnoreInstance>>(_ignoreInstances_ItemAdded);
            _ignoreInstances.ItemsRemoved += new EventHandler(_ignoreInstances_ItemsRemoved);
        }

        #region IProjectConfigurationStore Members

        public ICollection<string> TermTableFiles
        {
            get { return _termTableFiles; }
        }

        public ICollection<BuildTask.IIgnoreInstance> IgnoreInstances
        {
            get { return _ignoreInstances; }
        }

        public bool RunWithBuild
        {
            get { return false; }
            set { throw new InvalidOperationException(Resources.RunWithBuildForNonMSBuild); }
        }

        public bool HasConfiguration
        {
            get
            {
                Project dteProject = GetDTEProject(_project);
                if (dteProject == null)
                    return false;

                return dteProject.Globals.get_VariableExists(_termTablesName) || dteProject.Globals.get_VariableExists(_ignoreInstancesName);
            }
        }

        public void CreateDefaultConfiguration()
        {
            if (HasConfiguration)
            {
                throw new InvalidOperationException(Resources.AlreadyHasConfiguration);
            }

            _termTableFiles.Add(Globals.DefaultTermTablePath);
        }

        #endregion IProjectConfigurationStore Members

        #region Private Members

        readonly IServiceProvider _serviceProvider;
        readonly IVsProject _project;
        readonly CollectionWithEvents<string> _termTableFiles = new CollectionWithEvents<string>();
        readonly CollectionWithEvents<BuildTask.IIgnoreInstance> _ignoreInstances = new CollectionWithEvents<BuildTask.IIgnoreInstance>();

        void _ignoreInstances_ItemsRemoved(object sender, EventArgs e)
        {
            PersistIgnoreInstances();
        }

        void _ignoreInstances_ItemAdded(object sender, ItemEventArgs<BuildTask.IIgnoreInstance> e)
        {
            PersistIgnoreInstances();
        }

        void _termTableFiles_ItemsRemoved(object sender, EventArgs e)
        {
            PersistTermTables();
        }

        void _termTableFiles_ItemAdded(object sender, ItemEventArgs<string> e)
        {
            PersistTermTables();
        }

        private Project GetDTEProject(IVsProject project)
        {
            string projectPath = ProjectUtilities.GetProjectFilePath(project);

            var dte = _serviceProvider.GetService(typeof(DTE)) as DTE;
            if (dte == null)
            {
                Debug.Fail("Failed to get DTE service.");
                return null;
            }

            foreach (Project dteProject in dte.Solution.Projects)
            {
                if (String.Compare(dteProject.FileName, projectPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return dteProject;
                }
            }

            return null;
        }

        const string _termTablesName = "CodeSweep_TermTables";
        const string _ignoreInstancesName = "CodeSweep_IgnoreInstances";

        private void ReadConfigFromProject()
        {
            string projectFolder = Path.GetDirectoryName(ProjectUtilities.GetProjectFilePath(_project));
            Project dteProject = GetDTEProject(_project);
            if (dteProject == null)
                return;

            if (dteProject.Globals.get_VariableExists(_termTablesName))
            {
                string termTables = (string)dteProject.Globals[_termTablesName];

                foreach (string table in termTables.Split(';'))
                {
                    if (table != null && table.Length > 0)
                    {
                        if (Path.IsPathRooted(table))
                        {
                            _termTableFiles.Add(table);
                        }
                        else
                        {
                            _termTableFiles.Add(Utilities.AbsolutePathFromRelative(table, projectFolder));
                        }
                    }
                }
            }

            if (dteProject.Globals.get_VariableExists(_ignoreInstancesName))
            {
                string ignoreInstances = (string)dteProject.Globals[_ignoreInstancesName];
                _ignoreInstances.AddRange(BuildTask.Factory.DeserializeIgnoreInstances(ignoreInstances, projectFolder));
            }
        }

        private void PersistTermTables()
        {
            string projectFolder = Path.GetDirectoryName(ProjectUtilities.GetProjectFilePath(_project));
            List<string> relativePaths = Utilities.RelativizePathsIfPossible(_termTableFiles, projectFolder);

            Project dteProject = GetDTEProject(_project);
            if (dteProject == null)
                return;

            dteProject.Globals[_termTablesName] = Utilities.Concatenate(relativePaths, ";");
            dteProject.Globals.set_VariablePersists(_termTablesName, true);
        }

        private void PersistIgnoreInstances()
        {
            string projectFolder = Path.GetDirectoryName(ProjectUtilities.GetProjectFilePath(_project));
            string serialization = BuildTask.Factory.SerializeIgnoreInstances(_ignoreInstances, projectFolder);

            Project dteProject = GetDTEProject(_project);
            if (dteProject == null)
                return;

            dteProject.Globals[_ignoreInstancesName] = serialization;
            dteProject.Globals.set_VariablePersists(_ignoreInstancesName, true);
        }

        #endregion Private Members
    }
}
