/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace Microsoft.CommandTargetRGB
{
    static class PkgCmdIDList
    {
        public const uint cmdidShowToolWindow = 0x101;
        public const int cmdidRed = 0x102;
        public const int cmdidGreen = 0x103;
        public const int cmdidBlue = 0x104;
        public const int RGBToolbar = 0x2000;
        public const int RGBToolbarGroup = 0x2001;
    };
}