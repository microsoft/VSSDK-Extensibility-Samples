/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.Samples.VisualStudio.CodeSweep.Scanner;
using Microsoft.Samples.VisualStudio.CodeSweep.VSPackage.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    internal class BackgroundScanner : IBackgroundScanner
    {
        /// <summary>
        /// Creates a new background scanner.
        /// </summary>
        /// <param name="serviceProvider">The service provider that is used to get VS services.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>serviceProvider</c> is null.</exception>
        public BackgroundScanner(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }

            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Fired just before the background scan starts.  The call occurs on the thread on which
        /// <c>Start</c> is called, not on the background thread.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Fired after the background scan stops.  The call occurs on the background thread.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Starts a scan on a background thread.
        /// </summary>
        /// <param name="projects">The projects which will be scanned.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>project</c> is null.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if a background scan is already running.</exception>
        /// <remarks>
        /// Before this method returns, the CodeSweep task list will be cleared, and the tasklist
        /// window will be activated with the CodeSweep task list visible.
        /// While the scan is running, the status bar will display a progress indicator.
        /// As each file is processed, the results (if any) will be placed in the task list.
        /// If <c>project</c> does not contain a CodeSweep configuration, this method will return
        /// immediately without creating a background thread.  <c>Started</c> is not fired.
        /// </remarks>
        public void Start(IEnumerable<IVsProject> projects)
        {
            if (projects == null)
            {
                throw new ArgumentNullException("projects");
            }
            if (IsRunning)
            {
                throw new InvalidOperationException(Resources.BackgroundScanAlreadyRunning);
            }

            GatherProjectInfo(projects);

            StartWithExistingConfigs();
        }

        /// <summary>
        /// Starts a new scan, using the same project as the previous scan.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if a background scan is already running or if there was no previous scan to repeat.</exception>
        public void RepeatLast()
        {
            if (_projectsToScan.Count == 0)
            {
                throw new InvalidOperationException(Resources.NoPreviousScan);
            }

            StartWithExistingConfigs();
        }

        /// <summary>
        /// Gets a boolean value indicating whether a background scan is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Stops a scan in progress.
        /// </summary>
        /// <param name="blockUntilDone">Controls whether this method returns immediately or waits until the background thread has finished.</param>
        public void StopIfRunning(bool blockUntilDone)
        {
            ManualResetEvent doneEvent = new ManualResetEvent(false);
            if (blockUntilDone)
            {
                Stopped += (sender, e) => doneEvent.Set();
            }

            lock (_lockObj)
            {
                if (IsRunning)
                {
                    _stopPending = true;
                }
                else
                {
                    return;
                }
            }

            if (blockUntilDone)
            {
                doneEvent.WaitOne();
            }
        }

        #region Private members

        class ProjectConfiguration
        {
            readonly List<string> _filesToScan;
            readonly List<string> _termTableFiles;
            readonly string _projectPath;

            public ProjectConfiguration(IEnumerable<string> filesToScan, IEnumerable<string> termTableFiles, string projectPath)
            {
                _filesToScan = new List<string>(filesToScan);
                _termTableFiles = new List<string>(termTableFiles);
                _projectPath = projectPath;
            }

            public ICollection<string> FilesToScan
            {
                get { return _filesToScan; }
            }

            public ICollection<string> TermTableFiles
            {
                get { return _termTableFiles; }
            }

            public string ProjectPath
            {
                get { return _projectPath; }
            }
        }

        delegate IMultiFileScanResult ScanDelegate(
            IEnumerable<string> filePaths,
            IEnumerable<ITermTable> termTables,
            FileScanCompleted callback,
            FileContentGetter contentGetter,
            ScanStopper stopper);

        readonly IServiceProvider _serviceProvider;
        bool _stopPending = false;
        uint _filesProcessed = 0;
        uint _totalFiles = 0;
        uint _statusBarCookie = 0;
        List<IVsProject> _projectList = new List<IVsProject>();
        List<ProjectConfiguration> _projectsToScan = new List<ProjectConfiguration>();
        int _currentProject = 0;
        object _lockObj = new object();

        void ScanCompleted(IAsyncResult result)
        {
            lock (_lockObj)
            {
                ++_currentProject;
                if (_currentProject == _projectsToScan.Count || _stopPending)
                {
                    _currentProject = 0;
                    IsRunning = false;
                    _stopPending = false;
                    UpdateStatusBar(false, String.Empty, 0, 0);

                    var handler = Stopped;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
                else
                {
                    StartCurrentConfig();
                }
            }
        }

        void UpdateStatusBar(bool usingStatusBar, string text, uint soFar, uint total)
        {
            var statusBar = _serviceProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            if (statusBar != null)
            {
                statusBar.Progress(ref _statusBarCookie, usingStatusBar ? 1 : 0, text, soFar, total);
            }
            else
            {
                Debug.Fail("Failed to get SVsStatusbar service.");
            }

            if (!usingStatusBar)
            {
                _statusBarCookie = 0;
            }
        }

        void ScanResultRecieved(IScanResult result)
        {
            if (!_stopPending)
            {
                var taskProvider = Factory.GetTaskProvider() as TaskProvider;
                taskProvider.SetCurrentProject(_projectList[_currentProject]);
                Factory.GetTaskProvider().AddResult(result, _projectsToScan[_currentProject].ProjectPath);
                ++_filesProcessed;
                UpdateStatusBar(true, Resources.AppName, _filesProcessed, _totalFiles);
            }
        }

        void StartCurrentConfig()
        {
            ScanDelegate scanDelegate = CodeSweep.Scanner.Factory.GetScanner().Scan;

            List<ITermTable> termTables = new List<ITermTable>();
            foreach (string tableFile in _projectsToScan[_currentProject].TermTableFiles)
            {
                try
                {
                    termTables.Add(CodeSweep.Scanner.Factory.GetTermTable(tableFile));
                }
                catch (Exception ex)
                {
                    if (!(ex is ArgumentException || ex is System.Xml.XmlException))
                    {
                        throw;
                    }
                }
            }

            UpdateStatusBar(true, Resources.AppName, _filesProcessed, _totalFiles);

            var handler = Started;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            ScanStopper stopper = () => _stopPending;

            IsRunning = true;
            scanDelegate.BeginInvoke(
                _projectsToScan[_currentProject].FilesToScan,
                termTables,
                new FileScanCompleted(ScanResultRecieved),
                new FileContentGetter(GetTextOfFileIfOpenInIde),
                stopper,
                ScanCompleted,
                null /* 'object' argument */);
        }

        void StartWithExistingConfigs()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException(Resources.AlreadyScanning);
            }

            ResetScanRun();

            if (_totalFiles == 0)
            {
                return;
            }

            StartCurrentConfig();
        }

        void ResetScanRun()
        {
            _currentProject = 0;
            _filesProcessed = 0;

            Factory.GetTaskProvider().Clear();
            Factory.GetTaskProvider().ShowTaskList();
        }

        void GatherProjectInfo(IEnumerable<IVsProject> projects)
        {
            _projectsToScan.Clear();
            _totalFiles = 0;
            _projectList.Clear();
            int projectNumber = 0;

            foreach (IVsProject project in projects)
            {
                _projectList.Insert(projectNumber, project);
                projectNumber++;

                List<string> filesToScan = new List<string>();
                List<string> termTableFiles = new List<string>();

                if (Factory.GetProjectConfigurationStore(project).HasConfiguration)
                {
                    filesToScan.AddRange(Factory.GetBuildManager().AllItemsInProject(project));
                    termTableFiles.AddRange(Factory.GetProjectConfigurationStore(project).TermTableFiles);
                    _totalFiles += (uint)filesToScan.Count;
                }

                _projectsToScan.Add(new ProjectConfiguration(filesToScan, termTableFiles, ProjectUtilities.GetProjectFilePath(project)));
            }
        }

        public string GetTextOfFileIfOpenInIde(string filePath)
        {
            var rdt = _serviceProvider.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
            if (rdt == null)
            {
                Debug.Fail("Failed to get SVsRunningDocumentTable service.");
                return null;
            }

            IVsHierarchy hierarchy = null;
            uint itemid = 0;
            IntPtr docDataUnk = IntPtr.Zero;
            uint lockCookie = 0;

            int hr = rdt.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, filePath, out hierarchy, out itemid, out docDataUnk, out lockCookie);
            try
            {
                if (hr == VSConstants.S_OK)
                {
                    var textLines = Marshal.GetUniqueObjectForIUnknown(docDataUnk) as IVsTextLines;

                    if (textLines != null)
                    {
                        string text = null;
                        int endLine = 0;
                        int endIndex = 0;

                        hr = textLines.GetLastLineIndex(out endLine, out endIndex);
                        Debug.Assert(hr == VSConstants.S_OK, "GetLastLineIndex did not return S_OK.");

                        hr = textLines.GetLineText(0, 0, endLine, endIndex, out text);
                        Debug.Assert(hr == VSConstants.S_OK, "GetLineText did not return S_OK.");

                        return text;
                    }
                }

                return null;
            }
            finally
            {
                if (lockCookie != 0)
                {
                    rdt.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, lockCookie);
                }
            }
        }

        #endregion Private members
    }
}
