/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;

namespace Microsoft.Samples.VisualStudio.IDE.EditorWithToolbox
{
    /// <summary>
    /// This class contains a list of GUIDs specific to this sample, 
    /// especially the package GUID and the commands group GUID. 
    /// </summary>
    public static class GuidStrings
    {
        public const string GuidClientPackage = "68a4ede6-8f63-44f2-803e-65f770e709e1";
        public const string GuidClientCmdSet = "2513aa39-e57d-47d5-b6d1-a09061e103d7";
        public const string GuidEditorFactory = "93fa4dc3-61ec-47af-b0ba-50cad3caf049"; 
    }
    /// <summary>
    /// List of the GUID objects.
    /// </summary>
    internal static class GuidList
    {
        public static readonly Guid guidEditorCmdSet = new Guid(GuidStrings.GuidClientCmdSet);
        public static readonly Guid guidEditorFactory = new Guid(GuidStrings.GuidEditorFactory);
    };
}