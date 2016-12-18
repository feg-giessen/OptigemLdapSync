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
    internal partial class UserExecutor : UserControl
    {
        private const int ControlMargin = 6;

        private const int FormBottomMargin = 2;

        private bool allowAdd = true;

        private OptigemConnector optigem;

        private IList<Tuple<PersonModel, ListViewItem>> listedUsers = new List<Tuple<PersonModel, ListViewItem>>();

        private ListViewExtender playlistExtender;

        private bool textboxActive = false;

        public UserExecutor()
        {
            this.InitializeComponent();

            this.playlistExtender = new ListViewExtender(this.listView);

            // extend 2nd column
            var buttonAction = new ListViewButtonColumn(this.listView.Columns.Count - 1);
            buttonAction.Click += this.OnUserExecute;
            buttonAction.FixedWidth = true;

            this.playlistExtender.AddColumn(buttonAction);
        }

        private void OnUserExecute(object sender, ListViewColumnMouseEventArgs e)
        {
            if (this.UserAction != null)
            {
                PersonModel person = this.listedUsers.Where(x => x.Item2 == e.Item).Select(x => x.Item1).FirstOrDefault();

                this.UserAction(this, new UserEventArg(person));
            }
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

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public string ActionTitle { get; set; } = "Ausführen...";

        public event EventHandler<UserEventArg> UserAction;

        public void SetOptigemConnection(ISyncConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            this.optigem = new OptigemConnector(configuration.OptigemDatabasePath);
        }

        public void Add(PersonModel person)
        {
            ListViewItem item = this.listView.Items.Add(person.Nachname);
            item.SubItems.AddRange(new[] { person.Vorname, person.Username, this.ActionTitle });

            this.listedUsers.Add(new Tuple<PersonModel, ListViewItem>(person, item));
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
                    this.Add(person);
                }
            }
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            this.AllowAdd = this.optigem != null || this.DesignMode;
            this.btnAdd.Enabled = !string.IsNullOrWhiteSpace(this.txbAddUsername.Text);

            if (this.listedUsers.Count == 0)
            {
                this.txbAddUsername.Focus();
            }
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

    internal class UserEventArg : EventArgs
    {
        public UserEventArg(PersonModel person)
        {
            this.Person = person;
        }

        public PersonModel Person { get; private set; }
    }
}
