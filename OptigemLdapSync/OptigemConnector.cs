using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using Dapper;
using OptigemLdapSync.Models;

namespace OptigemLdapSync
{
    internal class OptigemConnector
    {
        private const string OptigemUser = "optigem";

        private const string OptigemPassword = "kyrios";

        private const string OptigemAddressDatabase = "m01-adr.mdb";

        private const string OptigemSystemDatabase = "optigemxp.mdw";

        private const string OptigemLdapSettingsField = "LdapSyncConnection";

        private const string PersonSelectString = @"
Spender.Nr, 
Spender.Vorname, 
Spender.Nachname, 
Spender.Briefanrede, 
Spender.bdF5 as [Username], 
Spender.bdf6 as [Password], 
Spender.bdF4 as SyncUserId, 
Spender.Titel, 
(Spender.Geschlecht + 1) as Geschlecht, 
Spender.[Beginn Mitgliedschaft] as StartDatum, 
Spender.[Ende Mitgliedschaft] as EndDatum, 
IIF(Spender.bdf7=0,FALSE,TRUE) as NoAddress, 
Spender.[Straße] as Strasse, 
Spender.[Telefon abends] as Telefon, 
Spender.Telefax, 
Spender.[Telefon mobil] as Mobiltelefon, 
Spender.EMail, 
Spender.PLZ as Plz, 
Spender.Ort, 
Spender.Zusatzort, 
Spender.Land, 
Spender.Geburtsdatum, 
Spender.OutlookAdrBuchAenderFlag as Aenderung";

        private readonly string connectionString;

        public OptigemConnector(string path)
        {
            string addressDatabase = Path.Combine(path, OptigemAddressDatabase);
            string systemDatabase = Path.Combine(path, OptigemSystemDatabase);

            if (!File.Exists(addressDatabase))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Texts.NoAddressDatabase, path, OptigemAddressDatabase), nameof(path));

            if (!File.Exists(systemDatabase))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Texts.NoSystemDatabase, path, OptigemSystemDatabase), nameof(path));

            this.connectionString =
                $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={addressDatabase};User ID={OptigemUser};Password={OptigemPassword};Jet OLEDB:System database={systemDatabase}";
        }

        public void SetCustomFieldsToNull()
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                connection.Execute("UPDATE Spender SET bdF4=Null WHERE bdF4=0");
                connection.Execute("UPDATE Spender SET bdF5=Null WHERE bdF5=''");
                connection.Execute("UPDATE Spender SET bdF6=Null WHERE bdF6=''");
            }
        }

        public int GetNextSyncUserId()
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                int lastId = (int)connection.ExecuteScalar("SELECT CLng(Max(bdF4)) FROM Spender WHERE bdF4 IS NOT NULL");

                return lastId + 1;
            }
        }

        public IEnumerable<PersonModel> GetNewPersons()
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                return connection.Query<PersonModel>(
                    "SELECT " + PersonSelectString + " FROM"
                    + " (Spender INNER JOIN [Personen-Kategorie-Zuordnung] ON Spender.Nr = [Personen-Kategorie-Zuordnung].[Personen-Nr])"
                    + " INNER JOIN Personenkategorien ON [Personen-Kategorie-Zuordnung].[Kategorie-ID] = Personenkategorien.ID"
                    + " WHERE (Personenkategorien.Präfix='Internetdienste.') AND (Spender.bdF4 Is Null)");
            }
        }

        public IEnumerable<PersonModel> GetAllPersons()
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                return connection.Query<PersonModel>("SELECT " + PersonSelectString + " FROM Spender WHERE Spender.bdF4 > 0");
            }
        }

        public IEnumerable<PersonModel> GetAllPersons(string username)
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                return connection.Query<PersonModel>("SELECT " + PersonSelectString + " FROM Spender WHERE Spender.bdF4 > 0 AND Spender.bdF5=@username", new { username });
            }
        }

        public IEnumerable<PersonModel> GetAllPersonsNotPrinted()
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                return connection.Query<PersonModel>("SELECT " + PersonSelectString + " FROM Spender WHERE Spender.bdF4 > 0 AND Spender.bdF9 is NULL");
            }
        }

        public IEnumerable<PersonModel> GetAllPersonsForPasswordMail()
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                return connection.Query<PersonModel>("SELECT " + PersonSelectString + " FROM Spender WHERE Spender.bdF4 > 0 AND Spender.bdF5 Is Not Null AND Spender.bdF6 Is Not Null AND Spender.bdF10 <> 0 AND Spender.bdF11 Is Null");
            }
        }

        public void SetPasswordMailDate(int personId)
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();
                
                connection.Execute(
                    "UPDATE Spender SET bdF11=Now() WHERE Nr=@personId",
                    new
                    {
                        personId
                    });
            }
        }

        public void SetPrintDate(int personId)
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();
                
                connection.Execute(
                    "UPDATE Spender SET bdF9=Now() WHERE Nr=@personId",
                    new
                    {
                        personId
                    });
            }
        }

        public IEnumerable<PersonKategorieZuordnung> GetAllKategorieZuordnungen()
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                return connection.Query<PersonKategorieZuordnung>("SELECT [Personen-Nr] as PersonId, [Kategorie-ID] as KategorieId FROM [Personen-Kategorie-Zuordnung]");
            }
        }

        public IEnumerable<PersonenkategorieModel> GetCategories()
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                foreach (var kategorie in connection.Query<PersonenkategorieModel>("SELECT ID as Id, Kurzname as Name, [Präfix] as Prefix FROM Personenkategorien WHERE [Präfix]='Internetdienste.'").ToList())
                {
                    kategorie.Personen.AddRange(
                        connection.Query<int>("SELECT [Personen-Nr] From [Personen-Kategorie-Zuordnung] WHERE [Kategorie-ID]=@Id", new { kategorie.Id }));

                    yield return kategorie;
                }
            }
        }

        public IEnumerable<PersonenkategorieModel> GetPersonenkategorien(int personNr)
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                return connection.Query<PersonenkategorieModel>(
                    "SELECT Personenkategorien.ID as Id, Personenkategorien.[Präfix] as Prefix, Personenkategorien.Kurzname as Name FROM"
                    + " (Spender INNER JOIN [Personen-Kategorie-Zuordnung] ON Spender.Nr = [Personen-Kategorie-Zuordnung].[Personen-Nr])"
                    + " INNER JOIN Personenkategorien ON [Personen-Kategorie-Zuordnung].[Kategorie-ID] = Personenkategorien.ID"
                    + " WHERE (Personenkategorien.Präfix='Internetdienste.') AND (Spender.Nr = @Nr)",
                    new { Nr = personNr });
            }
        }

        public IEnumerable<PersonModel> GetDeactivatedPersons()
        {
            const string sql = "SELECT " + PersonSelectString + " FROM"
                + " (Spender INNER JOIN [Personen-Kategorie-Zuordnung] ON Spender.Nr = [Personen-Kategorie-Zuordnung].[Personen-Nr])"
                + " INNER JOIN Personenkategorien ON [Personen-Kategorie-Zuordnung].[Kategorie-ID] = Personenkategorien.ID"
                + " WHERE Spender.Nr Not In (SELECT [Personen-Kategorie-Zuordnung].[Personen-Nr] FROM Personenkategorien INNER JOIN [Personen-Kategorie-Zuordnung] ON Personenkategorien.ID = [Personen-Kategorie-Zuordnung].[Kategorie-ID] WHERE Personenkategorien.Präfix='Internetdienste.')"
                + " AND Spender.bdF4 IS NOT NULL";

            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                return connection.Query<PersonModel>(sql);
            }
        }

        public void SetIntranetUserIds(IEnumerable<PersonModel> persons)
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                foreach (var person in persons)
                {
                    connection.Execute(
                        "UPDATE Spender SET bdF4=@Uid, bdF5=@Username, bdF6=@Password WHERE Nr=@Nr",
                        new
                        {
                            Uid = person.SyncUserId,
                            person.Username,
                            person.Password,
                            person.Nr
                        });
                }
            }
        }

        public void SetChangedFlag(int personId, bool value)
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                connection.Execute(
                    "UPDATE Spender SET OutlookAdrBuchAenderFlag=@value WHERE Nr=@personId",
                    new
                    {
                        value,
                        personId
                    });
            }
        }

        public string GetLdapConnectionString()
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                return connection
                    .Query<string>("SELECT Wert FROM [main_#_Optionen_Mandant] WHERE OptionsBez='" + OptigemLdapSettingsField + "'")
                    .FirstOrDefault();
            }
        }

        public void SetLdapConnectionString(string value)
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                if (connection.Query<string>("SELECT Wert FROM [main_#_Optionen_Mandant] WHERE OptionsBez='" + OptigemLdapSettingsField + "'").Any())
                {
                    connection.Execute("UPDATE [main_#_Optionen_Mandant] SET Wert=@value WHERE OptionsBez='" + OptigemLdapSettingsField + "'", new { value });
                }
                else
                {
                    connection.Execute("INSERT INTO [main_#_Optionen_Mandant] (OptionsBez, Wert) VALUES ('" + OptigemLdapSettingsField + "', @value)", new { value });
                }
            }
        }
    }
}
