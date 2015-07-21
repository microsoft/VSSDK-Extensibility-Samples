/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System.Windows.Forms;

namespace Microsoft.Samples.VisualStudio.IDE.WinformsControlsInstaller
{
    /// <summary>
    /// This is a simple custom control that will appear on the Winforms designer toolbox.
    /// </summary>
    public class MyCustomTextBox : TextBox
    {
        public MyCustomTextBox()
        {
            this.Multiline = true;
            this.Size = new System.Drawing.Size(100, 50);
        }
    }
}
