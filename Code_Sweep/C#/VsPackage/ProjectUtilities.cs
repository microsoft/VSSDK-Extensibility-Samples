/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    static class ProjectUtilities
    {
        private static IServiceProvider _serviceProvider;

        public static void SetServiceProvider(IServiceProvider provider)
        {
            _serviceProvider = provider;
        }

        static public IList<IVsProject> GetProjectsOfCurrentSelections()
        {
            List<IVsProject> results = new List<IVsProject>();

            int hr = VSConstants.S_OK;
            var selectionMonitor = _serviceProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            if (selectionMonitor == null)
            {
                Debug.Fail("Failed to get SVsShellMonitorSelection service.");
                return results;
            }

            IntPtr hierarchyPtr = IntPtr.Zero;
            uint itemID = 0;
            IVsMultiItemSelect multiSelect = null;
            IntPtr containerPtr = IntPtr.Zero;
            hr = selectionMonitor.GetCurrentSelection(out hierarchyPtr, out itemID, out multiSelect, out containerPtr);
            if (IntPtr.Zero != containerPtr)
            {
                Marshal.Release(containerPtr);
                containerPtr = IntPtr.Zero;
            }
            Debug.Assert(hr == VSConstants.S_OK, "GetCurrentSelection failed.");

            if (itemID == (uint)VSConstants.VSITEMID.Selection)
            {
                uint itemCount = 0;
                int fSingleHierarchy = 0;
                hr = multiSelect.GetSelectionInfo(out itemCount, out fSingleHierarchy);
                Debug.Assert(hr == VSConstants.S_OK, "GetSelectionInfo failed.");

                VSITEMSELECTION[] items = new VSITEMSELECTION[itemCount];
                hr = multiSelect.GetSelectedItems(0, itemCount, items);
                Debug.Assert(hr == VSConstants.S_OK, "GetSelectedItems failed.");

                foreach (VSITEMSELECTION item in items)
                {
                    IVsProject project = GetProjectOfItem(item.pHier, item.itemid);
                    if (!results.Contains(project))
                    {
                        results.Add(project);
                    }
                }
            }
            else
            {
                // case where no visible project is open (single file)
                if (hierarchyPtr != IntPtr.Zero)
                {
                    IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierarchyPtr);
                    results.Add(GetProjectOfItem(hierarchy, itemID));
                }
            }

            return results;
        }

        private static IVsProject GetProjectOfItem(IVsHierarchy hierarchy, uint itemID)
        {
            return (IVsProject)hierarchy;
        }

        static public string GetProjectFilePath(IVsProject project)
        {
            string path = string.Empty;
            int hr = project.GetMkDocument((uint)VSConstants.VSITEMID.Root, out path);
            Debug.Assert(hr == VSConstants.S_OK || hr == VSConstants.E_NOTIMPL, "GetMkDocument failed for project.");

            return path;
        }

        static public string GetUniqueProjectNameFromFile(string projectFile)
        {
            IVsProject project = GetProjectByFileName(projectFile);

            if (project != null)
            {
                return GetUniqueUIName(project);
            }

            return null;
        }

        static public string GetUniqueUIName(IVsProject project)
        {
            var solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution3;
            if (solution == null)
            {
                Debug.Fail("Failed to get SVsSolution service.");
                return null;
            }

            string name = null;
            int hr = solution.GetUniqueUINameOfProject((IVsHierarchy)project, out name);
            Debug.Assert(hr == VSConstants.S_OK, "GetUniqueUINameOfProject failed.");
            return name;
        }

        static public IEnumerable<IVsProject> LoadedProjects
        {
            get
            {
                var solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
                if (solution == null)
                {
                    Debug.Fail("Failed to get SVsSolution service.");
                    yield break;
                }

                IEnumHierarchies enumerator = null;
                Guid guid = Guid.Empty;
                solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref guid, out enumerator);
                IVsHierarchy[] hierarchy = new IVsHierarchy[1] { null };
                uint fetched = 0;
                for (enumerator.Reset(); enumerator.Next(1, hierarchy, out fetched) == VSConstants.S_OK && fetched == 1; /*nothing*/)
                {
                    yield return (IVsProject)hierarchy[0];
                }
            }
        }

        static public IVsProject GetProjectByFileName(string projectFile)
        {
            return LoadedProjects.FirstOrDefault(
                p => string.Compare(projectFile, GetProjectFilePath(p), StringComparison.OrdinalIgnoreCase) == 0);
        }

        static public bool IsMSBuildProject(IVsProject project)
        {
            return ProjectCollection.GlobalProjectCollection.GetLoadedProjects(GetProjectFilePath(project)).Any();
        }
    }
}
