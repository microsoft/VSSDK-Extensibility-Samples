/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

// Guids.cs
// MUST match guids.h
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Samples.VisualStudio.IronPython.Console
{
    public static class ConsoleGuidList
    {
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public const string guidIronPythonConsolePkgString =    "068980a2-def8-4422-adc4-76af7a935e7e";

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        public const string guidIronPythonConsoleCmdSetString = "aba8cb4c-73e3-4a11-8cde-9501d0a2ab9e";

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static readonly Guid guidIronPythonConsolePkg = new Guid(guidIronPythonConsolePkgString);

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        public static readonly Guid guidIronPythonConsoleCmdSet = new Guid(guidIronPythonConsoleCmdSetString);
    };
}