using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.Protocols;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OptigemLdapSync
{
    public partial class MainForm : Form, ITaskReporter
    {
        private ISyncConfiguration configuration;

        private readonly TempFilemanager tempFilemanager = new TempFilemanager();

        public MainForm()
        {
            this.InitializeComponent();

            this.configuration = new SyncConfiguration();
            this.tempFilemanager.Init(true);
        }

        private void OnSyncClick(object sender, EventArgs e)
        {
            var engine = new SyncEngine(this.configuration);

            try
            {
                engine.Do(this);
            }
            catch (LdapException exception)
            {
                MessageBox.Show("Es ist ein Fehler aufgetreten:" + Environment.NewLine + Environment.NewLine + exception.Message, "Fehler...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.ResetProgess();
            }            
        }

        private void OnSettingsClick(object sender, EventArgs e)
        {
            OptigemConnector optigem;

            try
            {
                optigem = new OptigemConnector(this.configuration.OptigemDatabasePath);
            }
            catch (ArgumentException)
            {
                optigem = null;
            }

            string connectionString = optigem?.GetLdapConnectionString();
            LdapConnectionStringParser ldapSettings = string.IsNullOrWhiteSpace(connectionString) ? null : new LdapConnectionStringParser() { ConnectionString = connectionString };

            var settingsForm = new SettingsForm
            {
                OptigemPath = this.configuration.OptigemDatabasePath,
                LdapServer = ldapSettings?.Server,
                LdapPort = ldapSettings?.Port ?? 636,
                LdapUser = ldapSettings?.User,
                LdapPassword = ldapSettings?.Password
            };

            settingsForm.FormClosing += (s, args) =>
            {
                var form = s as SettingsForm ?? settingsForm;
                if (form.DialogResult == DialogResult.OK)
                {
                    ldapSettings = new LdapConnectionStringParser
                    {
                        Server = settingsForm.LdapServer,
                        Port = settingsForm.LdapPort,
                        User = settingsForm.LdapUser,
                        Password = settingsForm.LdapPassword,
                    };

                    try
                    {
                        optigem = new OptigemConnector(settingsForm.OptigemPath);
                    }
                    catch (ArgumentException exception)
                    {
                        MessageBox.Show("Die Optigem-Datenbank konnte nicht geöffnet werden: " + Environment.NewLine + Environment.NewLine + exception.ToString(), "Fehler...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        args.Cancel = true;
                        return;
                    }

                    optigem.SetLdapConnectionString(ldapSettings.ToString());
                }
            };

            settingsForm.ShowDialog();
        }

        private void ResetProgess()
        {
            this.prgTasks.Value = this.prgTasks.Minimum;
            this.prgDetails.Value = this.prgDetails.Minimum;
            
            this.lblPrgTasks.Text = string.Empty;
            this.lblPrgDetails.Text = string.Empty;
        }

        public void Init(int taskCount)
        {
            this.prgTasks.Maximum = taskCount;
            this.prgTasks.Value = 0;

            this.lblPrgTasks.Text = "Start...";

            this.tvProtokol.Nodes.Clear();

            Application.DoEvents();
        }

        private TreeNode currentTaskNode;

        public void StartTask(string name, int total)
        {
            if (this.prgTasks.Value < this.prgTasks.Maximum)
            {
                this.prgTasks.Value += 1;
            }

            string label = $"[{this.prgTasks.Value}/{this.prgTasks.Maximum}] {name}";
            this.lblPrgTasks.Text = label;

            this.currentTaskNode = new TreeNode(label);
            this.tvProtokol.Nodes.Add(this.currentTaskNode);
            
            this.prgDetails.Maximum = total;
            this.prgDetails.Value = 0;

            this.lblPrgDetails.Text = string.Empty;
            Application.DoEvents();
        }

        private string detailItem;

        private TreeNode currentDetailNode;

        public void Progress(string text)
        {
            if (this.prgDetails.Value < this.prgDetails.Maximum)
            {
                this.prgDetails.Value += 1;
            }

            this.detailItem = text;
            this.currentDetailNode = null;

            this.lblPrgDetails.Text = $"[{this.prgDetails.Value}/{this.prgDetails.Maximum}] {text}";
            Application.DoEvents();
        }

        public void Log(string text)
        {
            if (this.currentDetailNode == null)
            {
                this.currentDetailNode = new TreeNode(this.detailItem);
                this.currentTaskNode.Nodes.Add(this.currentDetailNode);
            }

            this.currentDetailNode.Nodes.Add(text);
            Application.DoEvents();
        }

        private void OnPrintUserdataClicked(object sender, EventArgs e)
        {
            var form = new PrintForm();
                            
            form.ShowDialog();
        }

        private void OnPasswordMailClicked(object sender, EventArgs e)
        {
            var form = new PasswordMailForm();

            form.ShowDialog();
        }

        private void OnPasswordResetClicked(object sender, EventArgs e)
        {
            var form = new PasswordResetForm();

            form.ShowDialog();
        }
    }
}
