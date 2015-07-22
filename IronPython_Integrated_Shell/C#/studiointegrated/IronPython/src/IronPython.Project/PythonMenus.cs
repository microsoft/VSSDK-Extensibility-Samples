/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.ComponentModel.Design;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
    /// <summary>
    /// CommandIDs matching the commands defined symbols in PkgCmd.vsct
    /// </summary>
    public sealed class PythonMenus
    {
        internal static readonly Guid guidIronPythonProjectCmdSet = new Guid(GuidList.guidIronPythonProjectCmdSetString);
        internal static readonly CommandID SetAsMain = new CommandID(guidIronPythonProjectCmdSet, 0x3001);
    }
}

