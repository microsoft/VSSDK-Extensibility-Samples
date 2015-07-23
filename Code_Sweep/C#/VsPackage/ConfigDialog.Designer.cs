/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    partial class ConfigDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigDialog));
            this.label1 = new System.Windows.Forms.Label();
            this._termTableListBox = new System.Windows.Forms.ListBox();
            this._addButton = new System.Windows.Forms.Button();
            this._removeButton = new System.Windows.Forms.Button();
            this._closeButton = new System.Windows.Forms.Button();
            this._scanButton = new System.Windows.Forms.Button();
            this._autoScanCheckBox = new System.Windows.Forms.CheckBox();
            this._toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // _termTableListBox
            // 
            resources.ApplyResources(this._termTableListBox, "_termTableListBox");
            this._termTableListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this._termTableListBox.FormattingEnabled = true;
            this._termTableListBox.Name = "_termTableListBox";
            this._termTableListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this._termTableListBox.SelectedIndexChanged += new System.EventHandler(this._termTableListBox_SelectedIndexChanged);
            this._termTableListBox.ControlAdded += new System.Windows.Forms.ControlEventHandler(this._termTableListBox_ControlAdded);
            this._termTableListBox.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this._termTableListBox_ControlRemoved);
            // 
            // _addButton
            // 
            resources.ApplyResources(this._addButton, "_addButton");
            this._addButton.Name = "_addButton";
            this._addButton.Click += new System.EventHandler(this._addButton_Click);
            // 
            // _removeButton
            // 
            resources.ApplyResources(this._removeButton, "_removeButton");
            this._removeButton.Name = "_removeButton";
            this._removeButton.Click += new System.EventHandler(this._removeButton_Click);
            // 
            // _closeButton
            // 
            resources.ApplyResources(this._closeButton, "_closeButton");
            this._closeButton.Name = "_closeButton";
            this._closeButton.Click += new System.EventHandler(this._closeButton_Click);
            // 
            // _scanButton
            // 
            resources.ApplyResources(this._scanButton, "_scanButton");
            this._scanButton.Name = "_scanButton";
            this._scanButton.Click += new System.EventHandler(this._scanButton_Click);
            // 
            // _autoScanCheckBox
            // 
            resources.ApplyResources(this._autoScanCheckBox, "_autoScanCheckBox");
            this._autoScanCheckBox.Name = "_autoScanCheckBox";
            this._autoScanCheckBox.CheckedChanged += new System.EventHandler(this._autoScanCheckBox_CheckedChanged);
            // 
            // ConfigDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._autoScanCheckBox);
            this.Controls.Add(this._scanButton);
            this.Controls.Add(this._closeButton);
            this.Controls.Add(this._removeButton);
            this.Controls.Add(this._addButton);
            this.Controls.Add(this._termTableListBox);
            this.Controls.Add(this.label1);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ConfigDialog_KeyPress);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox _termTableListBox;
        private System.Windows.Forms.Button _addButton;
        private System.Windows.Forms.Button _removeButton;
        private System.Windows.Forms.Button _closeButton;
        private System.Windows.Forms.Button _scanButton;
        private System.Windows.Forms.CheckBox _autoScanCheckBox;
        private System.Windows.Forms.ToolTip _toolTip;
    }
}