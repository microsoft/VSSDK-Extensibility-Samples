/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.SccProvider
{
	/// <summary>
    /// Summary description for SccProviderOptionsControl.
	/// </summary>
	public class SccProviderOptionsControl : UserControl
    {
        private Label label1;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private Container components = null;
        // The parent page, use to persist data
        private SccProviderOptions _customPage;

        /// <summary>
        /// Constructor
        /// </summary>
        public SccProviderOptionsControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
				GC.SuppressFinalize(this);
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            label1 = new Label();
            SuspendLayout();

            // label1
            label1.AutoSize = true;
            label1.Location = new Point(13, 28);
            label1.Name = "label1";
            label1.Size = new Size(217, 13);
            label1.TabIndex = 2;
            label1.Text = "Sample source control provider options page";

            // SccProviderOptionsControl
            AllowDrop = true;
            Controls.Add(label1);
            Name = "SccProviderOptionsControl";
            Size = new Size(292, 195);
            ResumeLayout(false);
            PerformLayout();

		}
		#endregion
    
        public SccProviderOptions OptionsPage
        {
            set
            {
                _customPage = value;
            }
        }
    }

}
