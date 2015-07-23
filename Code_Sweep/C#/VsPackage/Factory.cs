/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.Samples.VisualStudio.CodeSweep.BuildTask;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    static class Factory
    {
        private static IServiceProvider _serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            set
            {
                _serviceProvider = value;
                ProjectUtilities.SetServiceProvider(_serviceProvider);
            }

            get { return _serviceProvider; }
        }

        public static IConfigurationDialog GetDialog()
        {
            ConfigDialog dialog = new ConfigDialog();
            dialog.ServiceProvider = _serviceProvider;
            return dialog;
        }

        private static Dictionary<IVsProject, IProjectConfigurationStore> _projectStores = new Dictionary<IVsProject, IProjectConfigurationStore>();

        public static IProjectConfigurationStore GetProjectConfigurationStore(IVsProject project)
        {

            if (_projectStores.ContainsKey(project))
            {
                return _projectStores[project];
            }
            else
            {
                IProjectConfigurationStore store;

                if (ProjectUtilities.IsMSBuildProject(project))
                {
                    store = new ProjectConfigStore(project);
                }
                else
                {
                    store = new NonMSBuildProjectConfigStore(project, _serviceProvider);
                }

                _projectStores.Add(project, store);
                return store;
            }
        }

        private static ITaskProvider _taskProvider;

        public static ITaskProvider GetTaskProvider()
        {
            if (_taskProvider == null)
            {
                _taskProvider = new TaskProvider(_serviceProvider);
            }
            return _taskProvider;
        }

        private static IBuildManager _buildManager;

        public static IBuildManager GetBuildManager()
        {
            if (_buildManager == null)
            {
                _buildManager = new BuildManager(_serviceProvider);
            }
            return _buildManager;
        }

        private static IBackgroundScanner _backgroundScanner;

        public static IBackgroundScanner GetBackgroundScanner()
        {
            if (_backgroundScanner == null)
            {
                _backgroundScanner = new BackgroundScanner(_serviceProvider);
            }
            return _backgroundScanner;
        }

        public static void CleanupFactory()
        {
            _backgroundScanner = null;
            _buildManager = null;
            _taskProvider = null;
            _projectStores.Clear();
        }
    }
}
