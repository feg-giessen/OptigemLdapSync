using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using OptigemLdapSync.Models;
using System.ComponentModel;

namespace OptigemLdapSync
{
    internal partial class UserSelector : UserControl
    {
        private const int ControlMargin = 6;

        private const int FormBottomMargin = 2;

        private bool allowAdd = true;

        private OptigemConnector optigem;

        private IList<Tuple<PersonModel, ListViewItem>> listedUsers = new List<Tuple<PersonModel, ListViewItem>>();

        public UserSelector()
        {
            this.InitializeComponent();
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public bool AllowAdd
        {
            get { return this.allowAdd; }

            set
            {
                if (this.allowAdd != value)
                {
                    if (value)
                    {
                        this.listView.Size = new Size(this.listView.Size.Width, this.Size.Height - ControlMargin - this.grpAdd.Height - FormBottomMargin);
                        this.grpAdd.Location = new Point(3, this.Size.Height - this.grpAdd.Height - FormBottomMargin);
                        this.grpAdd.Visible = true;
                    }
                    else
                    {
                        this.grpAdd.Visible = false;
                        this.listView.Size = new Size(this.listView.Size.Width, this.Size.Height - FormBottomMargin);
                    }

                    this.allowAdd = value;
                }
            }
        }

        public void SetOptigemConnection(ISyncConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            this.optigem = new OptigemConnector(configuration.OptigemDatabasePath);
        }

        public void Add(PersonModel person, bool selected = false)
        {
            ListViewItem item = this.listView.Items.Add(string.Empty);
            item.SubItems.AddRange(new[] { person.Nachname, person.Vorname, person.Username });
            item.Checked = selected;

            this.listedUsers.Add(new Tuple<PersonModel, ListViewItem>(person, item));
        }

        public IEnumerable<PersonModel> GetSelectedUsers()
        {
            return this.listedUsers.Where(x => x.Item2.Checked).Select(x => x.Item1);
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            if (this.optigem == null)
                return;

            IList<PersonModel> persons = this.optigem.GetAllPersons(this.txbAddUsername.Text?.Trim())
                .ToList();

            if (!persons.Any())
            {
                MessageBox.Show("Es konnte kein Benutzer mit dem angegebenen Benutzernamen gefunden werden.", "Fehler...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                foreach (PersonModel person in persons)
                {
                    this.Add(person, false);
                }
            }
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            this.AllowAdd = this.optigem != null || this.DesignMode;
            this.btnAdd.Enabled = !string.IsNullOrWhiteSpace(this.txbAddUsername.Text);
        }

        private void OnAddUsernameTextChanged(object sender, EventArgs e)
        {
            this.btnAdd.Enabled = !string.IsNullOrWhiteSpace(this.txbAddUsername.Text);
        }
    }
}
