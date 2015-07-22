/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Windows.Forms;

namespace Microsoft.Samples.VisualStudio.IDE.EditorWithToolbox
{
    /// <summary>
    /// This class provides room for extension of a RichTextBox.
    /// </summary>
    public class EditorControl : RichTextBox
    {
        #region Constructor
        /// <summary>
        /// Explicitly defined default constructor.
        /// Initialize new instance of the EditorControl.
        /// </summary>
        public EditorControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            WordWrap = false;
        }
        #endregion
    }
}
