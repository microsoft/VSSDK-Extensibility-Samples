/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.SccProvider
{
    /// <summary>
    /// Class definition for a dialog window that appears when the user edits a file that has not been checked out
    /// </summary>
    public partial class DlgQueryEditCheckedInFile : Form
    {
        public const int qecifCheckout = 1;
        public const int qecifEditInMemory = 2;
        public const int qecifCancelEdit = 3;

        int _answer = qecifCancelEdit;
        
        public int Answer
        {
            get { return _answer; }
            set { _answer = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">The name of the File to be Checked Out/Edited/Cancelled</param>
        public DlgQueryEditCheckedInFile(string filename)
        {
            InitializeComponent();

            // Format the message text with the current file name
            msgText.Text = string.Format(CultureInfo.CurrentUICulture, msgText.Text, filename);
        }

        /// <summary>
        /// Called when user presses 'Checkout' button from dialog
        /// </summary>
        private void btnCheckout_Click(object sender, EventArgs e)
        {
            Answer = qecifCheckout;
            Close();
        }

        /// <summary>
        /// Called when user presses 'Edit' button from dialog
        /// </summary>
        private void btnEdit_Click(object sender, EventArgs e)
        {
            Answer = qecifEditInMemory;
            Close();
        }

        /// <summary>
        /// Called when user presses 'Cancel' button from dialog
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Answer = qecifCancelEdit;
            Close();
        }
    }
}