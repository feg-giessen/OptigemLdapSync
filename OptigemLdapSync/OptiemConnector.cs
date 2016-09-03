using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using Dapper;

namespace OptigemLdapSync
{
    internal class OptigemConnector
    {
        private readonly string connectionString;

        public OptigemConnector(string path)
        {
            this.connectionString =
                $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=${Path.Combine(path, "m01-adr.mdb")};User ID=optigem;Password=kyrios;Jet OLEDB:System database=${Path.Combine(path, "optigemxp.mdw")}";
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

        public int GetNextIntranetUserId()
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                int lastId = (int)connection.ExecuteScalar("SELECT Max(bdF4) FROM Spender WHERE bdF4 IS NOT NULL");

                return lastId + 1;
            }
        }

        public IEnumerable<PersonIndexModel> GetNewPersons()
        {
            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                return connection.Query<PersonIndexModel>(
                    "SELECT Spender.Nr, Spender.Vorname, Spender.Nachname FROM"
                    + " (Spender INNER JOIN [Personen-Kategorie-Zuordnung] ON Spender.Nr = [Personen-Kategorie-Zuordnung].[Personen-Nr])"
                    + " INNER JOIN Personenkategorien ON [Personen-Kategorie-Zuordnung].[Kategorie-ID] = Personenkategorien.ID"
                    + " WHERE (Personenkategorien.Präfix='Internetdienste.') AND (Spender.bdF4 Is Null)");
            }
        }

        public IEnumerable<PersonIndexModel> GetPersonsWithRemovedCategories()
        {
            const string Sql = "SELECT Spender.Nr, Spender.Vorname, Spender.Nachname FROM"
                + " (Spender INNER JOIN [Personen-Kategorie-Zuordnung] ON Spender.Nr = [Personen-Kategorie-Zuordnung].[Personen-Nr])"
                + " INNER JOIN Personenkategorien ON [Personen-Kategorie-Zuordnung].[Kategorie-ID] = Personenkategorien.ID"
                + " WHERE Spender.Nr Not In (SELECT [Personen-Kategorie-Zuordnung].[Personen-Nr] FROM Personenkategorien INNER JOIN [Personen-Kategorie-Zuordnung] ON Personenkategorien.ID = [Personen-Kategorie-Zuordnung].[Kategorie-ID] WHERE Personenkategorien.Präfix='Internetdienste.')";

            using (var connection = new OleDbConnection(this.connectionString))
            {
                connection.Open();

                return connection.Query<PersonIndexModel>(Sql);
            }
        }

        public void SetIntranetUserIds(IEnumerable<PersonIndexModel> persons)
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
                            person.Nr,
                            person.Username,
                            person.Password,
                            Uid = person.IntranetUserId
                        });
                }
            }
        }
    }

    internal class PersonIndexModel
    {
        public int Nr { get; set; }

        public string Vorname { get; set; }

        public string Nachname { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int IntranetUserId { get; set; }
    }
}
