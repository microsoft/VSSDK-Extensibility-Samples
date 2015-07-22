/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

// Guids.cs
// MUST match guids.h
using System;

namespace MyCompany.RdtEventExplorer
{
    static class GuidsList
    {
        public const string guidRdtEventExplorerPkgString = "4881b54b-664b-4d84-91f3-494292e21010";
        public const string guidRdtEventExplorerCmdSetString = "f520383c-4ee3-4155-a499-2fe423f5e9e6";
        public const string guidToolWindowPersistanceString = "99cd759f-e9ab-4327-985a-040573ac417a";

        public static readonly Guid guidRdtEventExplorerPkg = new Guid(guidRdtEventExplorerPkgString);
        public static readonly Guid guidRdtEventExplorerCmdSet = new Guid(guidRdtEventExplorerCmdSetString);
        public static readonly Guid guidToolWindowPersistance = new Guid(guidToolWindowPersistanceString);
    };
}