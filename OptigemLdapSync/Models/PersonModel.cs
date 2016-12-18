using System;

namespace OptigemLdapSync.Models
{
    internal class PersonModel
    {
        public int Nr { get; set; }

        public string Vorname { get; set; }

        public string Nachname { get; set; }

        public string Briefanrede { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int SyncUserId { get; set; }

        public string Titel { get; set; }

        public Gender Geschlecht { get; set; }

        public DateTime? StartDatum { get; set; }

        public DateTime? EndDatum { get; set; }

        public bool NoAddress { get; set; }

        public string Strasse { get; set; }

        public string Plz { get; set; }

        public string Ort { get; set; }

        public string Zusatzort { get; set; }

        public string Land { get; set; }

        public string Telefon { get; set; }

        public string Telefax { get; set; }

        public string Mobiltelefon { get; set; }

        public string EMail { get; set; }

        public DateTime? Geburtsdatum { get; set; }

        public bool Aenderung { get; set; }
    }
}