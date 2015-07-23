/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    // MUST match VSPackage.vsct
    static class PkgCmdIDList
    {
        public const uint cmdidConfig = 0x100;
        public const uint cmdidStopScan = 0x101;
        public const uint cmdidRepeatLastScan = 0x102;
        public const uint cmdidIgnore = 0x103;
        public const uint cmdidDoNotIgnore = 0x104;
        public const uint cmdidShowIgnoredInstances = 0x105;
    }
}
