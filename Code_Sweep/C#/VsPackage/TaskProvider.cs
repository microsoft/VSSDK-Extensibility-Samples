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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    class TaskProvider : ITaskProvider, IVsSolutionEvents
    {
        public TaskProvider(IServiceProvider provider)
        {
            _imageList.ImageSize = new Size(9, 16);
            _imageList.Images.AddStrip(Resources.priority);
            _imageList.TransparentColor = Color.FromArgb(0, 255, 0);
            _serviceProvider = provider;
            var taskList = _serviceProvider.GetService(typeof(SVsTaskList)) as IVsTaskList;
            if (taskList == null)
            {
                Debug.Fail("Failed to get SVsTaskList service.");
                return;
            }

            int hr = taskList.RegisterTaskProvider(this, out _cookie);
            Debug.Assert(hr == VSConstants.S_OK, "RegisterTaskProvider did not return S_OK.");
            Debug.Assert(_cookie != 0, "RegisterTaskProvider did not return a nonzero cookie.");

            SetCommandHandlers();

            ListenForProjectUnload();
        }

        public void SetCurrentProject(IVsProject currentProject)
        {
            _currentProject = currentProject;
        }

        #region ITaskProvider Members

        public void AddResult(IScanResult result, string projectFile)
        {
            string fullPath = result.FilePath;
            if (!Path.IsPathRooted(fullPath))
            {
                fullPath = Utilities.AbsolutePathFromRelative(fullPath, Path.GetDirectoryName(projectFile));
            }

            if (result.Scanned)
            {
                foreach (IScanHit hit in result.Results)
                {
                    if (!string.IsNullOrEmpty(hit.Warning))
                    {
                        // See if we've warned about this term before; if so, don't warn again.
                        if (null == _termsWithDuplicateWarning.Find(
                            item => string.Compare(item, hit.Term.Text, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            _tasks.Add(new Task(hit.Term.Text, hit.Term.Severity, hit.Term.Class, hit.Warning, string.Empty, string.Empty, -1, -1, string.Empty, string.Empty, this, _serviceProvider, _currentProject));
                            _termsWithDuplicateWarning.Add(hit.Term.Text);
                        }
                    }

                    _tasks.Add(new Task(hit.Term.Text, hit.Term.Severity, hit.Term.Class, hit.Term.Comment, hit.Term.RecommendedTerm, fullPath, hit.Line, hit.Column, projectFile, hit.LineText, this, _serviceProvider, _currentProject));
                }
            }
            else
            {
                _tasks.Add(new Task(string.Empty, 1, string.Empty, string.Format(CultureInfo.CurrentUICulture, Resources.FileNotScannedError, fullPath), string.Empty, fullPath, -1, -1, projectFile, string.Empty, this, _serviceProvider, _currentProject));
            }
            Refresh();
        }

        public void Clear()
        {
            _tasks.Clear();
            Refresh();
        }

        public void ShowTaskList()
        {
            var shell = _serviceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (shell == null)
            {
                Debug.Fail("Failed to get SVsUIShell service.");
                return;
            }

            object dummy = null;
            Guid cmdSetGuid = VSConstants.GUID_VSStandardCommandSet97;
            int hr = shell.PostExecCommand(ref cmdSetGuid, (int)VSConstants.VSStd97CmdID.TaskListWindow, 0, ref dummy);
            Debug.Assert(hr == VSConstants.S_OK, "PostExecCommand did not return S_OK.");
        }

        /// <summary>
        /// Returns an image index between 0 and 2 inclusive corresponding to the specified severity.
        /// </summary>
        public static int GetImageIndexForSeverity(int severity)
        {
            return Math.Max(1, Math.Min(3, severity)) - 1;
        }

        public bool IsShowingIgnoredInstances { get; private set; }

        #endregion ITaskProvider Members

        #region IVsTaskProvider Members

        public int EnumTaskItems(out IVsEnumTaskItems ppenum)
        {
            ppenum = new TaskEnumerator(_tasks, IsShowingIgnoredInstances);
            return VSConstants.S_OK;
        }

        [DllImport("comctl32.dll")]
        static extern IntPtr ImageList_Duplicate(IntPtr original);

        public int ImageList(out IntPtr phImageList)
        {
            phImageList = ImageList_Duplicate(_imageList.Handle);
            return VSConstants.S_OK;
        }

        public int OnTaskListFinalRelease(IVsTaskList pTaskList)
        {
            if ((_cookie != 0) && (null != pTaskList))
            {
                int hr = pTaskList.UnregisterTaskProvider(_cookie);
                Debug.Assert(hr == VSConstants.S_OK, "UnregisterTaskProvider did not return S_OK.");
            }

            return VSConstants.S_OK;
        }

        public int ReRegistrationKey(out string pbstrKey)
        {
            pbstrKey = string.Empty;
            return VSConstants.E_NOTIMPL;
        }

        public int SubcategoryList(uint cbstr, string[] rgbstr, out uint pcActual)
        {
            pcActual = 0;
            return VSConstants.E_NOTIMPL;
        }

        #endregion IVsTaskProvider Members

        #region IVsTaskProvider3 Members

        public int GetColumn(int iColumn, VSTASKCOLUMN[] pColumn)
        {
            switch ((Task.TaskFields)iColumn)
            {
                case Task.TaskFields.Class:
                    pColumn[0].bstrCanonicalName = "Class";
                    pColumn[0].bstrHeading = Resources.ClassColumn;
                    pColumn[0].bstrLocalizedName = Resources.ClassColumn;
                    pColumn[0].bstrTip = string.Empty;
                    pColumn[0].cxDefaultWidth = 91;
                    pColumn[0].cxMinWidth = 0;
                    pColumn[0].fAllowHide = 1;
                    pColumn[0].fAllowUserSort = 1;
                    pColumn[0].fDescendingSort = 0;
                    pColumn[0].fDynamicSize = 1;
                    pColumn[0].fFitContent = 0;
                    pColumn[0].fMoveable = 1;
                    pColumn[0].fShowSortArrow = 1;
                    pColumn[0].fSizeable = 1;
                    pColumn[0].fVisibleByDefault = 1;
                    pColumn[0].iDefaultSortPriority = -1;
                    pColumn[0].iField = (int)Task.TaskFields.Class;
                    pColumn[0].iImage = -1;
                    break;
                case Task.TaskFields.Priority:
                    pColumn[0].bstrCanonicalName = "Priority";
                    pColumn[0].bstrHeading = "!";
                    pColumn[0].bstrLocalizedName = Resources.PriorityColumn;
                    pColumn[0].bstrTip = Resources.PriorityColumn;
                    pColumn[0].cxDefaultWidth = 22;
                    pColumn[0].cxMinWidth = 0;
                    pColumn[0].fAllowHide = 1;
                    pColumn[0].fAllowUserSort = 1;
                    pColumn[0].fDescendingSort = 0;
                    pColumn[0].fDynamicSize = 0;
                    pColumn[0].fFitContent = 0;
                    pColumn[0].fMoveable = 1;
                    pColumn[0].fShowSortArrow = 0;
                    pColumn[0].fSizeable = 1;
                    pColumn[0].fVisibleByDefault = 1;
                    pColumn[0].iDefaultSortPriority = -1;
                    pColumn[0].iField = (int)Task.TaskFields.Priority;
                    pColumn[0].iImage = -1;
                    break;
                case Task.TaskFields.PriorityNumber:
                    pColumn[0].bstrCanonicalName = "Priority Number";
                    pColumn[0].bstrHeading = "!#";
                    pColumn[0].bstrLocalizedName = Resources.PriorityNumberColumn;
                    pColumn[0].bstrTip = Resources.PriorityNumberColumn;
                    pColumn[0].cxDefaultWidth = 50;
                    pColumn[0].cxMinWidth = 0;
                    pColumn[0].fAllowHide = 1;
                    pColumn[0].fAllowUserSort = 1;
                    pColumn[0].fDescendingSort = 0;
                    pColumn[0].fDynamicSize = 0;
                    pColumn[0].fFitContent = 0;
                    pColumn[0].fMoveable = 1;
                    pColumn[0].fShowSortArrow = 0;
                    pColumn[0].fSizeable = 1;
                    pColumn[0].fVisibleByDefault = 0;
                    pColumn[0].iDefaultSortPriority = 0;
                    pColumn[0].iField = (int)Task.TaskFields.PriorityNumber;
                    pColumn[0].iImage = -1;
                    break;
                case Task.TaskFields.Replacement:
                    pColumn[0].bstrCanonicalName = "Replacement";
                    pColumn[0].bstrHeading = Resources.ReplacementColumn;
                    pColumn[0].bstrLocalizedName = Resources.ReplacementColumn;
                    pColumn[0].bstrTip = string.Empty;
                    pColumn[0].cxDefaultWidth = 140;
                    pColumn[0].cxMinWidth = 0;
                    pColumn[0].fAllowHide = 1;
                    pColumn[0].fAllowUserSort = 1;
                    pColumn[0].fDescendingSort = 0;
                    pColumn[0].fDynamicSize = 0;
                    pColumn[0].fFitContent = 0;
                    pColumn[0].fMoveable = 1;
                    pColumn[0].fShowSortArrow = 1;
                    pColumn[0].fSizeable = 1;
                    pColumn[0].fVisibleByDefault = 0;
                    pColumn[0].iDefaultSortPriority = -1;
                    pColumn[0].iField = (int)Task.TaskFields.Replacement;
                    pColumn[0].iImage = -1;
                    break;
                case Task.TaskFields.Term:
                    pColumn[0].bstrCanonicalName = "Term";
                    pColumn[0].bstrHeading = Resources.TermColumn;
                    pColumn[0].bstrLocalizedName = Resources.TermColumn;
                    pColumn[0].bstrTip = string.Empty;
                    pColumn[0].cxDefaultWidth = 103;
                    pColumn[0].cxMinWidth = 0;
                    pColumn[0].fAllowHide = 1;
                    pColumn[0].fAllowUserSort = 1;
                    pColumn[0].fDescendingSort = 0;
                    pColumn[0].fDynamicSize = 1;
                    pColumn[0].fFitContent = 0;
                    pColumn[0].fMoveable = 1;
                    pColumn[0].fShowSortArrow = 1;
                    pColumn[0].fSizeable = 1;
                    pColumn[0].fVisibleByDefault = 1;
                    pColumn[0].iDefaultSortPriority = -1;
                    pColumn[0].iField = (int)Task.TaskFields.Term;
                    pColumn[0].iImage = -1;
                    break;
                default:
                    return VSConstants.E_INVALIDARG;
            }

            return VSConstants.S_OK;
        }

        public int GetColumnCount(out int pnColumns)
        {
            pnColumns = Enum.GetValues(typeof(Task.TaskFields)).Length;
            return VSConstants.S_OK;
        }

        public int GetProviderFlags(out uint tpfFlags)
        {
            tpfFlags = (uint)(__VSTASKPROVIDERFLAGS.TPF_NOAUTOROUTING | __VSTASKPROVIDERFLAGS.TPF_ALWAYSVISIBLE);
            return VSConstants.S_OK;
        }

        public int GetProviderGuid(out Guid pguidProvider)
        {
            pguidProvider = _providerGuid;
            return VSConstants.S_OK;
        }

        public int GetProviderName(out string pbstrName)
        {
            pbstrName = Resources.AppName;
            return VSConstants.S_OK;
        }

        public int GetProviderToolbar(out Guid pguidGroup, out uint pdwID)
        {
            pguidGroup = GuidList.guidVSPackageCmdSet;
            pdwID = 0x2020;
            return VSConstants.S_OK;
        }

        public int GetSurrogateProviderGuid(out Guid pguidProvider)
        {
            pguidProvider = Guid.Empty;
            return VSConstants.E_NOTIMPL;
        }

        public int OnBeginTaskEdit(IVsTaskItem pItem)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnEndTaskEdit(IVsTaskItem pItem, int fCommitChanges, out int pfAllowChanges)
        {
            pfAllowChanges = 0;
            return VSConstants.E_NOTIMPL;
        }

        #endregion IVsTaskProvider3 Members

        #region IVsSolutionEvents Members

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            string projFile = ProjectUtilities.GetProjectFilePath(pHierarchy as IVsProject);

            if (!string.IsNullOrEmpty(projFile))
            {
                // Remove all tasks for the project that is being closed.
                for (int i = 0; i < _tasks.Count; ++i)
                {
                    if (_tasks[i].ProjectFile == projFile)
                    {
                        _tasks.RemoveAt(i);
                        --i;
                    }
                }

                Refresh();
            }

            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        #endregion IVsSolutionEvents Members

        #region Private Members

        static readonly Guid _providerGuid = new Guid("{9ACC41B7-15B4-4dd7-A0F3-0A935D5647F3}");

        readonly List<Task> _tasks = new List<Task>();
        readonly IServiceProvider _serviceProvider;
        readonly uint _cookie;
        readonly List<string> _termsWithDuplicateWarning = new List<string>();
        readonly ImageList _imageList = new ImageList();
        uint _solutionEventsCookie = 0;
        IVsProject _currentProject;

        private void ListenForProjectUnload()
        {
            var solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (solution == null)
            {
                Debug.Fail("Failed to get SVsSolution service.");
                return;
            }

            int hr = solution.AdviseSolutionEvents(this, out _solutionEventsCookie);
            Debug.Assert(hr == VSConstants.S_OK, "AdviseSolutionEvents did not return S_OK.");
            Debug.Assert(_solutionEventsCookie != 0, "AdviseSolutionEvents did not return a nonzero cookie.");
        }

        private void SetCommandHandlers()
        {
            var mcs = _serviceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs == null)
            {
                Debug.Fail("Failed to get IMenuCommandService service.");
                return;
            }

            CommandID ignoreID = new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidIgnore);
            OleMenuCommand ignoreCommand = new OleMenuCommand(new EventHandler(IgnoreSelectedItems), new EventHandler(QueryIgnore), ignoreID);
            mcs.AddCommand(ignoreCommand);
            ignoreCommand.BeforeQueryStatus += new EventHandler(QueryIgnore);

            CommandID dontIgnoreID = new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidDoNotIgnore);
            OleMenuCommand dontIgnoreCommand = new OleMenuCommand(new EventHandler(DontIgnoreSelectedItems), new EventHandler(QueryDontIgnore), dontIgnoreID);
            mcs.AddCommand(dontIgnoreCommand);
            dontIgnoreCommand.BeforeQueryStatus += new EventHandler(QueryDontIgnore);

            CommandID showIgnoredID = new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidShowIgnoredInstances);
            OleMenuCommand showIgnoredCommand = new OleMenuCommand(new EventHandler(ToggleShowIgnoredInstances), showIgnoredID);
            mcs.AddCommand(showIgnoredCommand);
        }

        List<Task> GetSelectedTasks()
        {
            var result = new List<Task>();

            int hr = VSConstants.S_OK;
            var taskList = _serviceProvider.GetService(typeof(SVsTaskList)) as IVsTaskList2;
            if (taskList == null)
            {
                Debug.Fail("Failed to get SVsTaskList service.");
                return result;
            }

            IVsEnumTaskItems enumerator = null;
            hr = taskList.EnumSelectedItems(out enumerator);
            Debug.Assert(hr == VSConstants.S_OK, "EnumSelectedItems did not return S_OK.");

            IVsTaskItem[] items = new IVsTaskItem[] { null };
            uint[] fetched = new uint[] { 0 };
            for (enumerator.Reset(); enumerator.Next(1, items, fetched) == VSConstants.S_OK && fetched[0] == 1; /*nothing*/)
            {
                Task task = items[0] as Task;
                if (task != null)
                {
                    result.Add(task);
                }
            }

            return result;
        }

        private void QueryIgnore(object sender, EventArgs e)
        {
            var mcs = _serviceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs == null)
            {
                Debug.Fail("Failed to get IMenuCommandService service.");
                return;
            }

            MenuCommand command = mcs.FindCommand(new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidIgnore));
            
            bool anyNotIgnored = GetSelectedTasks().Any(t => !t.Ignored);

            command.Supported = true;
            command.Enabled = anyNotIgnored;
        }

        private void QueryDontIgnore(object sender, EventArgs e)
        {
            var mcs = _serviceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs == null)
            {
                Debug.Fail("Failed to get IMenuCommandService service.");
                return;
            }

            MenuCommand command = mcs.FindCommand(new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidDoNotIgnore));

            bool anyIgnored = GetSelectedTasks().Any(t => t.Ignored);

            command.Supported = true;
            command.Enabled = anyIgnored;
        }

        private void IgnoreSelectedItems(object sender, EventArgs e)
        {
            SetSelectedItemsIgnoreState(true);
        }

        private void DontIgnoreSelectedItems(object sender, EventArgs e)
        {
            SetSelectedItemsIgnoreState(false);
        }

        private void SetSelectedItemsIgnoreState(bool ignore)
        {
            foreach (Task task in GetSelectedTasks())
            {
                task.Ignored = ignore;
            }
            Refresh();
        }

        private void ToggleShowIgnoredInstances(object sender, EventArgs e)
        {
            IsShowingIgnoredInstances = !IsShowingIgnoredInstances;

            var mcs = _serviceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs != null)
            {
                mcs.FindCommand(new CommandID(GuidList.guidVSPackageCmdSet, (int)PkgCmdIDList.cmdidShowIgnoredInstances)).Checked = IsShowingIgnoredInstances;
            }
            else
            {
                Debug.Fail("Failed to get IMenuCommandService service.");
            }

            Refresh();
        }

        private void Refresh()
        {
            var taskList = _serviceProvider.GetService(typeof(SVsTaskList)) as IVsTaskList;
            if (taskList == null)
            {
                return;
            }

            int hr = taskList.RefreshTasks(_cookie);
            Debug.Assert(hr == VSConstants.S_OK, "RefreshTasks did not return S_OK.");
        }

        #endregion Private Members
    }
}
