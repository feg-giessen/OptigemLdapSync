using System;
using System.Collections.Generic;
using System.Linq;

namespace OptigemLdapSync
{
    internal class SyncEngine
    {
        private OptigemConnector optigem = new OptigemConnector(@"C:\temp\fegt3");

        public void Do()
        {
            // 0. Set bdf4=Null where bdf4=0
            this.optigem.SetCustomFieldsToNull();

            // 1. IntranetNr und -PW vergeben
            int intranetUid = this.optigem.GetNextIntranetUserId();

            IList<PersonIndexModel> persons = this.optigem.GetNewPersons().ToList();
            foreach (var person in persons)
            {
                string username = (person.Vorname?.Trim() + "." + person.Nachname?.Trim()).Trim('.');

                person.Username = username.Length > 50 ? username.Substring(0, 50) : username;
                person.Password = this.CalculatePassword();
                person.IntranetUserId = intranetUid++;
            }

            this.optigem.SetIntranetUserIds(persons);

            // 2. Nicht-Mitglied-Benutzer deaktivieren
        }

        private string CalculatePassword()
        {
            const int PinLaenge = 8;
            const string CharList1 = "abcdefghijklmnopqrstuvwxyz";
            const string CharList2 = "1234567890";

            string tmpDlPin = string.Empty;
            int i;

            Random rand = new Random(DateTime.UtcNow.Millisecond);

            for (i = 0; i < PinLaenge; i++)
            {
                int pos;
                if (i < 4)
                {
                    do
                    {
                        pos = (int)(CharList1.Length * rand.NextDouble());
                    }
                    while (pos < 0 || pos >= CharList1.Length);

                    tmpDlPin = tmpDlPin + CharList1.Substring(pos, 1);
                }
                else
                {
                    do
                    {
                        pos = (int)(CharList2.Length * rand.NextDouble());
                    }
                    while (pos < 0 || pos >= CharList2.Length);

                    tmpDlPin = tmpDlPin + CharList2.Substring(pos, 1);
                }
            }

            return tmpDlPin;
        }
    }
}
