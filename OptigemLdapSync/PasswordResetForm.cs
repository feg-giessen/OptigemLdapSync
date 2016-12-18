using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Windows.Forms;
using OptigemLdapSync.Models;

namespace OptigemLdapSync
{
    public partial class PasswordResetForm : Form
    {
        private readonly ISyncConfiguration configuration;

        private bool textboxActive;

        public PasswordResetForm()
        {
            this.InitializeComponent();

            this.configuration = new SyncConfiguration();
        }

        private void OnOkClicked(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnPasswordSetClicked(object sender, EventArgs e)
        {
            var optigem = new OptigemConnector(this.configuration.OptigemDatabasePath);
            
            IList<PersonModel> persons = optigem.GetAllPersons(this.txbAddUsername.Text?.Trim())
                .ToList();

            if (!persons.Any())
            {
                MessageBox.Show("Es konnte kein Benutzer mit dem angegebenen Benutzernamen gefunden werden.", "Fehler...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (persons.Count() > 1)
            {
                MessageBox.Show("Es wurde mehr als ein Benutzer mit dem angegebenen Benutzernamen gefunden.", "Fehler...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var person = persons.First();

                var parser = new LdapConnectionStringParser { ConnectionString = optigem.GetLdapConnectionString() };
                var ldap = new LdapConnector(parser.Server, parser.Port, AuthType.Basic, parser.User, parser.Password, 100, this.configuration.WhatIf);

                SearchResultEntry[] searchResult = ldap
                    .PagedSearch($"(&(objectClass=inetOrgPerson)(syncUserId={person.SyncUserId}))", this.configuration.LdapDirectoryBaseDn, LdapBuilder.AllAttributes)
                    .SelectMany(s => s.OfType<SearchResultEntry>())
                    .ToArray();

                if (searchResult.Length == 0)
                {
                    MessageBox.Show("Der ausgewählte Benutzer ist in LDAP nicht vorhanden." + Environment.NewLine + "Das Passwort kann nicht gesetzt werden.", "Fehler...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (searchResult.Length > 1)
                {
                    MessageBox.Show("Für den Benutzer ist mehr als ein Eintrag in LDAP vorhanden." + Environment.NewLine + "Das Passwort kann nicht gesetzt werden.", "Fehler...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    var entry = searchResult.First();

                    var password = new DirectoryAttribute("userpassword", LdapBuilder.GenerateSaltedSha1(person.Password));
                    bool result = ldap.ModifyEntry(entry.DistinguishedName, new DirectoryAttributeModification[] { password.CreateModification(DirectoryAttributeOperation.Replace) });

                    if (result)
                    {
                        MessageBox.Show("Das Passwort für den Benutzer wurde zurück gesetzt.", "Passwort zurückgesetzt...", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Beim Zurücksetzen des Passworts is ein Fehler aufgetreten.", "Fehler...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            this.btnAdd.Enabled = !string.IsNullOrWhiteSpace(this.txbAddUsername.Text);
        }

        private void OnAddUsernameTextChanged(object sender, EventArgs e)
        {
            this.btnAdd.Enabled = !string.IsNullOrWhiteSpace(this.txbAddUsername.Text);
        }

        private void OnTextboxEnter(object sender, EventArgs e)
        {
            this.textboxActive = true;
        }

        private void OnTextboxLeave(object sender, EventArgs e)
        {
            this.textboxActive = false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (this.textboxActive && keyData == Keys.Enter)
            {
                this.btnAdd.PerformClick();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
