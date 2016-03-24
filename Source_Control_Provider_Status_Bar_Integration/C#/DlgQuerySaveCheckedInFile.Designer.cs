/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.SccProvider
{
    partial class DlgQuerySaveCheckedInFile
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.msgText = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnCheckout = new System.Windows.Forms.Button();
            this.btnSaveAs = new System.Windows.Forms.Button();
            this.btnSkipSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // msgText
            // 
            this.msgText.Location = new System.Drawing.Point(-1, 9);
            this.msgText.Name = "msgText";
            this.msgText.Size = new System.Drawing.Size(517, 51);
            this.msgText.TabIndex = 1;
            this.msgText.Text = "The read only file {0} is under source control and checked in.What do you want to" +
                " do?";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(482, 72);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(73, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnCheckout
            // 
            this.btnCheckout.Location = new System.Drawing.Point(11, 72);
            this.btnCheckout.Name = "btnCheckout";
            this.btnCheckout.Size = new System.Drawing.Size(150, 23);
            this.btnCheckout.TabIndex = 4;
            this.btnCheckout.Text = "Checkout the file and save it";
            this.btnCheckout.UseVisualStyleBackColor = true;
            this.btnCheckout.Click += new System.EventHandler(this.btnCheckout_Click);
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.Location = new System.Drawing.Point(297, 72);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(174, 23);
            this.btnSaveAs.TabIndex = 6;
            this.btnSaveAs.Text = "Save the file with different name";
            this.btnSaveAs.UseVisualStyleBackColor = true;
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // btnSkipSave
            // 
            this.btnSkipSave.Location = new System.Drawing.Point(172, 72);
            this.btnSkipSave.Name = "btnSkipSave";
            this.btnSkipSave.Size = new System.Drawing.Size(114, 23);
            this.btnSkipSave.TabIndex = 7;
            this.btnSkipSave.Text = "Do not save this file";
            this.btnSkipSave.UseVisualStyleBackColor = true;
            this.btnSkipSave.Click += new System.EventHandler(this.btnSkipSave_Click);
            // 
            // DlgQuerySaveCheckedInFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(567, 108);
            this.Controls.Add(this.btnSkipSave);
            this.Controls.Add(this.btnSaveAs);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCheckout);
            this.Controls.Add(this.msgText);
            this.Name = "DlgQuerySaveCheckedInFile";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Microsoft Visual Studio";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label msgText;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnCheckout;
        private System.Windows.Forms.Button btnSaveAs;
        private System.Windows.Forms.Button btnSkipSave;
    }
}