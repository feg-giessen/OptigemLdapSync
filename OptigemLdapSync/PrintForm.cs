using System;
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
