/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.Build.Construction;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    internal delegate void EmptyEvent();

    interface IBuildManager
    {
        event EmptyEvent BuildStarted;
        event EmptyEvent BuildStopped;

        bool IsListeningToBuildEvents { get; set; }
        ProjectTaskElement GetBuildTask(IVsProject project, bool createIfNecessary);
        IEnumerable<string> AllItemsInProject(IVsProject project);
        void SetProperty(IVsProject project, string name, string value);
        string GetProperty(IVsProject project, string name);
        void CreatePerUserFilesAsNecessary();
    }
}
