/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.Samples.VisualStudio.CodeSweep.BuildTask;
using Microsoft.Samples.VisualStudio.CodeSweep.Scanner;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    class ScannerHost : MarshalByRefObject, IScannerHost
    {
        public override object InitializeLifetimeService()
        {
            return null; // infinite lifetime
        }

        #region IScannerHost Members

        public void AddResult(IScanResult result, string projectFile)
        {
            Factory.GetTaskProvider().AddResult(result, projectFile);
        }

        #endregion IScannerHost Members
    }
}
