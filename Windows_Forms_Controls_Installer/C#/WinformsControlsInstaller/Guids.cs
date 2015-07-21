/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;

namespace Microsoft.Samples.VisualStudio.IDE.WinformsControlsInstaller
{
    static class GuidList
    {
        public const string guidWinformsControlsInstallerPkgString = "bbdf2b9d-b9bc-4d0c-8480-f46c68806fe2";
        public const string guidWinformsControlsInstallerCmdSetString = "58a3c677-9903-43ba-b991-df7093aaf841";

        public static readonly Guid guidWinformsControlsInstallerCmdSet = new Guid(guidWinformsControlsInstallerCmdSetString);
    };
}