/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.Build.Construction;
using Microsoft.Samples.VisualStudio.CodeSweep.VSPackage.Properties;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    /// <remarks>
    /// If the project referenced by this object is unloaded, this object will NOT detect the
    /// condition and update itself.  Therefore, you must create a new ProjectConfigStore in that
    /// situation.  Factory.GetProjectConfigurationStore takes care of this; it will return a new
    /// store object if the project has been unloaded and reloaded.
    /// </remarks>
    class ProjectConfigStore : IProjectConfigurationStore
    {
        /// <summary>
        /// Creates a new project configuration store object.
        /// </summary>
        /// <param name="project">The project whose state this store will reflect.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>project</c> is null.</exception>
        public ProjectConfigStore(IVsProject project)
        {
            _project = project;

            _buildTask = Factory.GetBuildManager().GetBuildTask(project, false);
            if (_buildTask != null)
            {
                ReadConfigFromBuildTask();
            }

            _termTableFiles.ItemAdded += new EventHandler<ItemEventArgs<string>>(_termTableFiles_ItemAdded);
            _termTableFiles.ItemsRemoved += new EventHandler(_termTableFiles_ItemsRemoved);
            _ignoreInstances.ItemAdded += new EventHandler<ItemEventArgs<BuildTask.IIgnoreInstance>>(_ignoreInstances_ItemAdded);
            _ignoreInstances.ItemsRemoved += new EventHandler(_ignoreInstances_ItemsRemoved);
        }

        #region IProjectConfigurationStore Members

        /// <summary>
        /// Gets the (read-write) collection of term table files for this project.
        /// </summary>
        public ICollection<string> TermTableFiles
        {
            get
            {
                return _termTableFiles;
            }
        }

        /// <summary>
        /// Gets the (read-write) collection of ignore instances for this project.
        /// </summary>
        public ICollection<BuildTask.IIgnoreInstance> IgnoreInstances
        {
            get
            {
                return _ignoreInstances;
            }
        }

        /// <summary>
        /// Gets or sets the boolean value controlling whether the scan will be run as part of the
        /// build process.
        /// </summary>
        public bool RunWithBuild
        {
            get
            {
                string propVal = Factory.GetBuildManager().GetProperty(_project, BuildManager.RunWithBuildFlag);
                return propVal != null && propVal == "true";
            }
            set
            {
                string propVal = value ? "true" : "false";
                Factory.GetBuildManager().SetProperty(_project, BuildManager.RunWithBuildFlag, propVal);
            }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the current project contains a CodeSweep configuration.
        /// </summary>
        public bool HasConfiguration
        {
            get { return _buildTask != null; }
        }

        /// <summary>
        /// Creates the default configuration for a project which does not have one.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the project already has a configuration.</exception>
        public void CreateDefaultConfiguration()
        {
            if (_buildTask != null)
            {
                throw new InvalidOperationException(Resources.AlreadyHasConfiguration);
            }
            CreateBuildTaskIfNecessary();
        }

        #endregion IProjectConfigurationStore Members

        #region Private Members

        readonly IVsProject _project;
        ProjectTaskElement _buildTask;
        readonly CollectionWithEvents<string> _termTableFiles = new CollectionWithEvents<string>();
        readonly CollectionWithEvents<BuildTask.IIgnoreInstance> _ignoreInstances = new CollectionWithEvents<BuildTask.IIgnoreInstance>();

        void PersistIgnoreInstances()
        {
            string projectFolder = Path.GetDirectoryName(ProjectUtilities.GetProjectFilePath(_project));
            string serialization = BuildTask.Factory.SerializeIgnoreInstances(_ignoreInstances, projectFolder);

            CreateBuildTaskIfNecessary();
            _buildTask.SetParameter("IgnoreInstances", serialization);
        }

        void PersistTermTables()
        {
            CreateBuildTaskIfNecessary();

            string projectFolder = Path.GetDirectoryName(ProjectUtilities.GetProjectFilePath(_project));
            List<string> relativePaths = Utilities.RelativizePathsIfPossible(_termTableFiles, projectFolder);

            _buildTask.SetParameter("TermTables", Utilities.Concatenate(relativePaths, ";"));
        }

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

        void CreateBuildTaskIfNecessary()
        {
            if (_buildTask == null)
            {
                _buildTask = Factory.GetBuildManager().GetBuildTask(_project, true);
                ReadConfigFromBuildTask();
            }
        }

        private void ReadConfigFromBuildTask()
        {
            string projectFolder = Path.GetDirectoryName(ProjectUtilities.GetProjectFilePath(_project));

            _ignoreInstances.AddRange(BuildTask.Factory.DeserializeIgnoreInstances(_buildTask.GetParameter("IgnoreInstances"), projectFolder));

            foreach (string termTable in _buildTask.GetParameter("TermTables").Split(';'))
            {
                if (termTable != null && termTable.Length > 0)
                {
                    if (Path.IsPathRooted(termTable))
                    {
                        _termTableFiles.Add(termTable);
                    }
                    else
                    {
                        _termTableFiles.Add(Utilities.AbsolutePathFromRelative(termTable, projectFolder));
                    }
                }
            }
        }

        #endregion Private Members
    }
}
