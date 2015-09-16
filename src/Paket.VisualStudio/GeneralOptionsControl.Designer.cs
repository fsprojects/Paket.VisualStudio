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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.autoRestoreCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.autoRestoreCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(4, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(349, 55);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Package Restore";
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
            // GeneralOptionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "GeneralOptionControl";
            this.Size = new System.Drawing.Size(361, 79);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox autoRestoreCheckBox;
    }
}
