using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace OptigemLdapSync
{
    public partial class PasswordMailForm : Form
    {
        private readonly ISyncConfiguration configuration;

        public PasswordMailForm()
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
                var body = new StringBuilder();
                body
                    .AppendLine(e.Person.Briefanrede + ",")
                    .AppendLine()
                    .AppendLine("hier kommt der zweite Teil der Zugangsdaten, das Passwort, das zum Login fürs FeG-Intranet benötigt wird.")
                    .AppendLine()
                    .AppendLine("Das Passwort (bitte vertraulich behandeln) lautet: " + e.Person.Password)
                    .AppendLine("In ein paar Stunden ist der Zugang freigeschaltet.")
                    .AppendLine()
                    .AppendLine("Auf der Startseite im Intranet, rechts unten, kann unter \"Mein Benutzerkonto\" das Passwort geändert werden.")
                    .AppendLine()
                    .AppendLine("Viel Freude beim Lesen der internen Seiten.");

                string mailto = "mailto:" + e.Person.EMail + "?subject=FeG Intranet&body=" + Uri.EscapeUriString(body.ToString());
                Process.Start(mailto);
                success = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Bei der Erstellung des Formulars ist ein Fehler aufgetreten:" + Environment.NewLine + Environment.NewLine + exception.ToString(), "Fehler...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (success && this.chkSetDate.Checked)
            {
                var optigem = new OptigemConnector(this.configuration.OptigemDatabasePath);
                optigem.SetPasswordMailDate(e.Person.Nr);
            }
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            var optigem = new OptigemConnector(this.configuration.OptigemDatabasePath);

            foreach (var person in optigem.GetAllPersonsForPasswordMail())
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
