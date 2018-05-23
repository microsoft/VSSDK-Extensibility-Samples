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

        public DlgQuerySaveCheckedInFile(string filename)
        {
            InitializeComponent();

            // Format the message text with the current file name
            msgText.Text = String.Format(CultureInfo.CurrentUICulture, msgText.Text, filename);
        }

        private void btnCheckout_Click(object sender, EventArgs e)
        {
            Answer = qscifCheckout;
            Close();
        }

        private void btnSkipSave_Click(object sender, EventArgs e)
        {
            Answer = qscifSkipSave;
            Close();
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            Answer = qscifForceSaveAs;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Answer = qscifCancel;
            Close();
        }
    }
}