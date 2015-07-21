/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System.ComponentModel;
using System.Windows.Forms;

namespace Microsoft.Samples.VisualStudio.IDE.WinformsControlsInstaller
{
    /// <summary>
    /// This Winforms control has a custom ToolboxItem that displays a wizard.
    /// To prevent the control from being installed in the toolbox, uncomment
    /// [ToolboxItem(false)] and comment [ToolboxItem(typeof(MyToolboxItem))].
    /// </summary>
    // [ToolboxItem(false)]
    [ToolboxItem(typeof(MyToolboxItem))]
    public class MyCustomTextBoxWithPopup : TextBox
    {
    }
}
