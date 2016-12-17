using System;
using System.IO;
using System.Windows.Forms;

namespace OptigemLdapSync
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            this.InitializeComponent();
        }

        public string OptigemPath
        {
            get { return this.txbOptigemPath.Text; }
            set { this.txbOptigemPath.Text = value; }
        }

        public string LdapServer
        {
            get { return this.txbLdapServer.Text; }
            set { this.txbLdapServer.Text = value; }
        }

        public int LdapPort
        {
            get { return (int)this.nudLdapPort.Value; }
            set { this.nudLdapPort.Value = value; }
        }

        public string LdapUser
        {
            get { return this.txbLdapUser.Text; }
            set { this.txbLdapUser.Text = value; }
        }

        public string LdapPassword
        {
            get { return this.mtbPassword.Text; }
            set { this.mtbPassword.Text = value; }
        }

        private void OnBtnBrowseClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.txbOptigemPath.Text) && Directory.Exists(this.txbOptigemPath.Text))
            {
                this.folderBrowserDialog.SelectedPath = this.txbOptigemPath.Text;
            }

            if (this.folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.txbOptigemPath.Text = this.folderBrowserDialog.SelectedPath;
            }
        }

        private void OnBtnOKClicked(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnBtnCancelClicked(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
