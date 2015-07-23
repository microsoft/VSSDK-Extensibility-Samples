/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.Samples.VisualStudio.CodeSweep.Scanner;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    interface ITaskProvider : IVsTaskProvider, IVsTaskProvider3
    {
        void AddResult(IScanResult result, string projectFile);
        void Clear();
        void ShowTaskList();
        bool IsShowingIgnoredInstances { get; }
    }
}
