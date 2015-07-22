/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;

namespace Microsoft.Samples.VisualStudio.ComboBox
{
    static class GuidList
    {
        public const string guidComboBoxPkgString = "40d9f297-25fb-4264-99ed-7785f8331c94";
        public const string guidComboBoxCmdSetString = "B2C8E135-0E7A-4696-963E-BD3280F8578C";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Guid guidComboBoxPkg = new Guid(guidComboBoxPkgString);
        public static readonly Guid guidComboBoxCmdSet = new Guid(guidComboBoxCmdSetString);
    };
}