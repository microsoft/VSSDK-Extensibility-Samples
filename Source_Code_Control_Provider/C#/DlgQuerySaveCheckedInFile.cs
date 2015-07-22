/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.SccProvider
{
    /// <summary>
    /// Class definition for a dialog window that appears when the user saves a file that has not been checked out
    /// </summary>
    public partial class DlgQuerySaveCheckedInFile : Form
    {
        public const int qscifCheckout = 1;
        public const int qscifSkipSave = 2;
        public const int qscifForceSaveAs = 3;
        public const int qscifCancel = 4;

        int _answer = qscifCancel;

        public int Answer
        {
            get { return _answer; }
            set { _answer = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">The name of the file that is being saved</param>
        public DlgQuerySaveCheckedInFile(string filename)
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
            Answer = qscifCheckout;
            Close();
        }

        /// <summary>
        /// Called when user presses 'Skip' button from dialog
        /// </summary>
        private void btnSkipSave_Click(object sender, EventArgs e)
        {
            Answer = qscifSkipSave;
            Close();
        }

        /// <summary>
        /// Called when user presses 'Change Name' button from dialog
        /// </summary>
        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            Answer = qscifForceSaveAs;
            Close();
        }

        /// <summary>
        /// Called when user presses 'Cancel' button from dialog
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Answer = qscifCancel;
            Close();
        }
    }
}