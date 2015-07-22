/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace MyCompany.RdtEventExplorer
{
    static class PkgCmdIDList
    {
        public const uint cmdidMyTool = 0x2001;
        public const int cmdidClearWindowsList = 0x2002;
        public const int cmdidRefreshWindowsList = 0x2003;

        // Define the list of menus (these include toolbars)
        public const int IDM_MyToolbar = 0x0101;
    };
}