/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    interface IBackgroundScanner
    {
        event EventHandler Started;
        event EventHandler Stopped;

        bool IsRunning { get; }
        void Start(IEnumerable<IVsProject> projects);
        void RepeatLast();
        void StopIfRunning(bool blockUntilDone);
    }
}
