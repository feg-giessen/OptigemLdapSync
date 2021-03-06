﻿using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace OptigemLdapSync
{
    public partial class PrintForm : Form
    {
        private readonly ISyncConfiguration configuration;

        public PrintForm()
        {
            this.InitializeComponent();

            this.configuration = new SyncConfiguration();
            this.userSelector.SetOptigemConnection(this.configuration);
            this.userSelector.UserAction += OnPrintClicked;
        }

        internal TempFilemanager TempFileManager { get; set; }

        private void OnPrintClicked(object sender, UserEventArg e)
        {
            if (e?.Person == null)
                return;

            if (this.TempFileManager == null)
            {
                this.TempFileManager = new TempFilemanager();
                this.TempFileManager.Init();
            }

            bool success = false;
            try
            {
                var report = new NewUserReport(this.TempFileManager);
                string reportName = report.Create(e.Person);

                Process.Start(reportName);
                success = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Bei der Erstellung des Formulars ist ein Fehler aufgetreten:" + Environment.NewLine + Environment.NewLine + exception.ToString(), "Fehler...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (success && this.chkSetDate.Checked)
            {
                var optigem = new OptigemConnector(this.configuration.OptigemDatabasePath);
                optigem.SetPrintDate(e.Person.Nr);
            }
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            var optigem = new OptigemConnector(this.configuration.OptigemDatabasePath);

            foreach (var person in optigem.GetAllPersonsNotPrinted())
            {
                this.userSelector.Add(person);
            }
        }

        private void OnOkClicked(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
