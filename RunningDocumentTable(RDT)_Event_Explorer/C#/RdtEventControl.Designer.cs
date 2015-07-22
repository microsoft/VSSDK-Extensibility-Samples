/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

namespace MyCompany.RdtEventExplorer
{
    partial class RdtEventControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            this.eventGrid = new System.Windows.Forms.DataGridView();
            this.detailsColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.myControlBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.eventGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.myControlBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // eventGrid
            // 
            this.eventGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.eventGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.eventGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.eventGrid.Location = new System.Drawing.Point(0, 0);
            this.eventGrid.Name = "eventGrid";
            this.eventGrid.Size = new System.Drawing.Size(389, 255);
            this.eventGrid.TabIndex = 1;
            this.eventGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.eventGrid_CellClick);
            // 
            // detailsColumn
            // 
            this.detailsColumn.Name = "detailsColumn";
            // 
            // myControlBindingSource
            // 
            this.myControlBindingSource.DataSource = typeof(MyCompany.RdtEventExplorer.RdtEventControl);
            // 
            // RDTEventControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.eventGrid);
            this.Name = "RDTEventControl";
            this.Size = new System.Drawing.Size(389, 293);
            ((System.ComponentModel.ISupportInitialize)(this.eventGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.myControlBindingSource)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.BindingSource myControlBindingSource;
        private System.Windows.Forms.DataGridView eventGrid;
        private System.Windows.Forms.DataGridViewButtonColumn detailsColumn;
    }
}
