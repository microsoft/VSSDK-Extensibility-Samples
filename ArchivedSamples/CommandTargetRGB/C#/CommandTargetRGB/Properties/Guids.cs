/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.CommandTargetRGB
{
    static class GuidList
    {
        public const string guidCommandTargetRGBPkgString = "a7dcebeb-e498-4d25-a554-e59003a2b7ee";
        public const string guidCommandTargetRGBCmdSetString = "00dcae55-4379-40a6-b152-3a38de753f29";
        public const string guidToolWindowPersistenceString = "0bdb1e08-ed8b-47e8-91b2-e9bd814b4ebb";

        public static readonly Guid guidCommandTargetRGBCmdSet = new Guid(guidCommandTargetRGBCmdSetString);
    };
}