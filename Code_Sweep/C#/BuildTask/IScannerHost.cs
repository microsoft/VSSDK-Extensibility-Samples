/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.Build.Framework;
using Microsoft.Samples.VisualStudio.CodeSweep.Scanner;
using System.Runtime.InteropServices;

namespace Microsoft.Samples.VisualStudio.CodeSweep.BuildTask
{
    /// <summary>
    /// The interface implemented by the host object the CodeSweep VS package sets for the scanner
    /// build tasks.
    /// </summary>
    public interface IScannerHost
    {
        /// <summary>
        /// Adds the results of a file scan to the task list.
        /// </summary>
        /// <param name="result">The results of the file scan.</param>
        /// <param name="projectFile">The full path of the project file.</param>
        void AddResult(IScanResult result, string projectFile);
    }
}
