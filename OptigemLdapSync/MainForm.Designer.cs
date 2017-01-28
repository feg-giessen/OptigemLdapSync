namespace OptigemLdapSync
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            this.tempFilemanager.Dispose();

            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.prgTasks = new System.Windows.Forms.ProgressBar();
            this.prgDetails = new System.Windows.Forms.ProgressBar();
            this.lblPrgTasks = new System.Windows.Forms.Label();
            this.lblPrgDetails = new System.Windows.Forms.Label();
            this.btnSync = new System.Windows.Forms.Button();
            this.btnSendPasswordMail = new System.Windows.Forms.Button();
            this.btnPrintUserdata = new System.Windows.Forms.Button();
            this.btnResetPassword = new System.Windows.Forms.Button();
            this.tvProtokol = new System.Windows.Forms.TreeView();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSettings = new System.Windows.Forms.Button();
            this.pcbLogo = new System.Windows.Forms.PictureBox();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pcbLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // prgTasks
            // 
            this.prgTasks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.prgTasks.Location = new System.Drawing.Point(12, 211);
            this.prgTasks.Name = "prgTasks";
            this.prgTasks.Size = new System.Drawing.Size(590, 23);
            this.prgTasks.TabIndex = 0;
            // 
            // prgDetails
            // 
            this.prgDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.prgDetails.Location = new System.Drawing.Point(12, 262);
            this.prgDetails.Name = "prgDetails";
            this.prgDetails.Size = new System.Drawing.Size(590, 23);
            this.prgDetails.TabIndex = 0;
            // 
            // lblPrgTasks
            // 
            this.lblPrgTasks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPrgTasks.BackColor = System.Drawing.Color.Transparent;
            this.lblPrgTasks.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrgTasks.Location = new System.Drawing.Point(12, 191);
            this.lblPrgTasks.Name = "lblPrgTasks";
            this.lblPrgTasks.Size = new System.Drawing.Size(590, 17);
            this.lblPrgTasks.TabIndex = 1;
            this.lblPrgTasks.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPrgDetails
            // 
            this.lblPrgDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPrgDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrgDetails.Location = new System.Drawing.Point(12, 243);
            this.lblPrgDetails.Name = "lblPrgDetails";
            this.lblPrgDetails.Size = new System.Drawing.Size(590, 16);
            this.lblPrgDetails.TabIndex = 1;
            this.lblPrgDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSync
            // 
            this.btnSync.Location = new System.Drawing.Point(12, 105);
            this.btnSync.Name = "btnSync";
            this.btnSync.Size = new System.Drawing.Size(100, 42);
            this.btnSync.TabIndex = 0;
            this.btnSync.Text = "Synchronisieren";
            this.btnSync.UseVisualStyleBackColor = true;
            this.btnSync.Click += new System.EventHandler(this.OnSyncClick);
            // 
            // btnSendPasswordMail
            // 
            this.btnSendPasswordMail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSendPasswordMail.Location = new System.Drawing.Point(396, 105);
            this.btnSendPasswordMail.Name = "btnSendPasswordMail";
            this.btnSendPasswordMail.Size = new System.Drawing.Size(100, 42);
            this.btnSendPasswordMail.TabIndex = 2;
            this.btnSendPasswordMail.Text = "Passwort-Mail senden...";
            this.btnSendPasswordMail.UseVisualStyleBackColor = true;
            this.btnSendPasswordMail.Click += new System.EventHandler(this.OnPasswordMailClicked);
            // 
            // btnPrintUserdata
            // 
            this.btnPrintUserdata.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPrintUserdata.Location = new System.Drawing.Point(290, 105);
            this.btnPrintUserdata.Name = "btnPrintUserdata";
            this.btnPrintUserdata.Size = new System.Drawing.Size(100, 42);
            this.btnPrintUserdata.TabIndex = 1;
            this.btnPrintUserdata.Text = "Benutzerdaten drucken...";
            this.btnPrintUserdata.UseVisualStyleBackColor = true;
            this.btnPrintUserdata.Click += new System.EventHandler(this.OnPrintUserdataClicked);
            // 
            // btnResetPassword
            // 
            this.btnResetPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetPassword.Location = new System.Drawing.Point(502, 105);
            this.btnResetPassword.Name = "btnResetPassword";
            this.btnResetPassword.Size = new System.Drawing.Size(100, 42);
            this.btnResetPassword.TabIndex = 3;
            this.btnResetPassword.Text = "Passwort zurücksetzen...";
            this.btnResetPassword.UseVisualStyleBackColor = true;
            this.btnResetPassword.Click += new System.EventHandler(this.OnPasswordResetClicked);
            // 
            // tvProtokol
            // 
            this.tvProtokol.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvProtokol.Location = new System.Drawing.Point(12, 291);
            this.tvProtokol.Name = "tvProtokol";
            this.tvProtokol.Size = new System.Drawing.Size(590, 279);
            this.tvProtokol.TabIndex = 5;
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.SystemColors.Window;
            this.pnlTop.Controls.Add(this.label2);
            this.pnlTop.Controls.Add(this.label1);
            this.pnlTop.Controls.Add(this.pcbLogo);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(614, 91);
            this.pnlTop.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label2.Location = new System.Drawing.Point(18, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Freie ev. Gemeinde Gießen";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.Highlight;
            this.label1.Location = new System.Drawing.Point(16, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(307, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "OPTIGEM -> LDAP Abgleich";
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(12, 153);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(100, 24);
            this.btnSettings.TabIndex = 4;
            this.btnSettings.Text = "Einstellungen...";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.OnSettingsClick);
            // 
            // pcbLogo
            // 
            this.pcbLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pcbLogo.Image = global::OptigemLdapSync.Properties.Resources.OptigemLdapSyncLogo;
            this.pcbLogo.Location = new System.Drawing.Point(398, 10);
            this.pcbLogo.Name = "pcbLogo";
            this.pcbLogo.Size = new System.Drawing.Size(207, 69);
            this.pcbLogo.TabIndex = 0;
            this.pcbLogo.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 582);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.tvProtokol);
            this.Controls.Add(this.btnPrintUserdata);
            this.Controls.Add(this.btnResetPassword);
            this.Controls.Add(this.btnSendPasswordMail);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnSync);
            this.Controls.Add(this.prgTasks);
            this.Controls.Add(this.prgDetails);
            this.Controls.Add(this.lblPrgDetails);
            this.Controls.Add(this.lblPrgTasks);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(540, 450);
            this.Name = "MainForm";
            this.Text = "OPTIGEM -> LDAP Abgleich";
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pcbLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar prgTasks;
        private System.Windows.Forms.ProgressBar prgDetails;
        private System.Windows.Forms.Label lblPrgTasks;
        private System.Windows.Forms.Label lblPrgDetails;
        private System.Windows.Forms.Button btnSync;
        private System.Windows.Forms.Button btnSendPasswordMail;
        private System.Windows.Forms.Button btnPrintUserdata;
        private System.Windows.Forms.Button btnResetPassword;
        private System.Windows.Forms.TreeView tvProtokol;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.PictureBox pcbLogo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSettings;
    }
}

