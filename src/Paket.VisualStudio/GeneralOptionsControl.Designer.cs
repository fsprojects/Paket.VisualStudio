namespace Paket.VisualStudio
{
    public partial class GeneralOptionControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpPackageRestore = new System.Windows.Forms.GroupBox();
            this.autoRestoreCheckBox = new System.Windows.Forms.CheckBox();
            this.grpAPIKeys = new System.Windows.Forms.GroupBox();
            this.dgvAPIKeys = new System.Windows.Forms.DataGridView();
            this.colSrcURL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grpPackageRestore.SuspendLayout();
            this.grpAPIKeys.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAPIKeys)).BeginInit();

            this.SuspendLayout();
            // 
            // grpPackageRestore
            // 
            this.grpPackageRestore.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPackageRestore.Controls.Add(this.autoRestoreCheckBox);
            this.grpPackageRestore.Location = new System.Drawing.Point(4, 8);
            this.grpPackageRestore.Name = "grpPackageRestore";
            this.grpPackageRestore.Size = new System.Drawing.Size(525, 49);
            this.grpPackageRestore.TabIndex = 1;
            this.grpPackageRestore.TabStop = false;
            this.grpPackageRestore.Text = "Package Restore";
            // 
            // autoRestoreCheckBox
            // 
            this.autoRestoreCheckBox.Location = new System.Drawing.Point(6, 19);
            this.autoRestoreCheckBox.Name = "autoRestoreCheckBox";
            this.autoRestoreCheckBox.Size = new System.Drawing.Size(307, 24);
            this.autoRestoreCheckBox.TabIndex = 3;
            this.autoRestoreCheckBox.Text = "Automatically restore packages during build in Visual Studio";
            this.autoRestoreCheckBox.UseVisualStyleBackColor = true;
            // 
            // grpAPIKeys
            // 
            this.grpAPIKeys.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpAPIKeys.Controls.Add(this.dgvAPIKeys);
            this.grpAPIKeys.Location = new System.Drawing.Point(6, 67);
            this.grpAPIKeys.Name = "grpAPIKeys";
            this.grpAPIKeys.Size = new System.Drawing.Size(523, 147);
            this.grpAPIKeys.TabIndex = 2;
            this.grpAPIKeys.TabStop = false;
            this.grpAPIKeys.Text = "API Keys";
            // 
            // dgvAPIKeys
            // 
            this.dgvAPIKeys.AllowUserToResizeRows = false;
            this.dgvAPIKeys.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvAPIKeys.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAPIKeys.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAPIKeys.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSrcURL,
            this.colKey});
            this.dgvAPIKeys.Location = new System.Drawing.Point(9, 19);
            this.dgvAPIKeys.MultiSelect = false;
            this.dgvAPIKeys.Name = "dgvAPIKeys";
            this.dgvAPIKeys.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAPIKeys.Size = new System.Drawing.Size(508, 118);
            this.dgvAPIKeys.TabIndex = 3;
            // 
            // colSrcURL
            // 
            this.colSrcURL.DataPropertyName = "colSrcURL";
            this.colSrcURL.HeaderText = "URL";
            this.colSrcURL.Name = "colSrcURL";
            // 
            // colKey
            // 
            this.colKey.DataPropertyName = "colKey";
            this.colKey.HeaderText = "Key";
            this.colKey.Name = "colKey";
            // 
            // GeneralOptionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpAPIKeys);
            this.Controls.Add(this.grpPackageRestore);
            this.Name = "GeneralOptionControl";
            this.Size = new System.Drawing.Size(532, 219);
            this.grpPackageRestore.ResumeLayout(false);
            this.grpAPIKeys.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAPIKeys)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox grpPackageRestore;
        private System.Windows.Forms.CheckBox autoRestoreCheckBox;
        private System.Windows.Forms.GroupBox grpAPIKeys;
        private System.Windows.Forms.DataGridView dgvAPIKeys;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSrcURL;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKey;
    }
}
