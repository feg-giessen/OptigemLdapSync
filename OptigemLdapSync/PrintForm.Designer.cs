namespace OptigemLdapSync
{
    partial class PrintForm
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
            this.btnOK = new System.Windows.Forms.Button();
            this.chkSetDate = new System.Windows.Forms.CheckBox();
            this.userSelector = new OptigemLdapSync.UserExecutor();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnOK.Location = new System.Drawing.Point(428, 319);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.OnOkClicked);
            // 
            // chkSetDate
            // 
            this.chkSetDate.AutoSize = true;
            this.chkSetDate.Checked = true;
            this.chkSetDate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSetDate.Location = new System.Drawing.Point(17, 12);
            this.chkSetDate.Name = "chkSetDate";
            this.chkSetDate.Size = new System.Drawing.Size(201, 17);
            this.chkSetDate.TabIndex = 2;
            this.chkSetDate.Text = "Druckdatum in Benutzerdaten setzen";
            this.chkSetDate.UseVisualStyleBackColor = true;
            // 
            // userSelector
            // 
            this.userSelector.ActionTitle = "Drucken...";
            this.userSelector.AllowAdd = true;
            this.userSelector.Location = new System.Drawing.Point(12, 35);
            this.userSelector.Name = "userSelector";
            this.userSelector.Size = new System.Drawing.Size(491, 278);
            this.userSelector.TabIndex = 3;
            // 
            // PrintForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnOK;
            this.ClientSize = new System.Drawing.Size(515, 354);
            this.Controls.Add(this.userSelector);
            this.Controls.Add(this.chkSetDate);
            this.Controls.Add(this.btnOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PrintForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Benutzerdaten drucken...";
            this.Load += new System.EventHandler(this.OnLoaded);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckBox chkSetDate;
        private UserExecutor userSelector;
    }
}