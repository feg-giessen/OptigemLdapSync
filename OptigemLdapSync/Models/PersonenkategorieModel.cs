using System.Collections.Generic;

namespace OptigemLdapSync.Models
{
    public class PersonenkategorieModel
    {
        public int Id { get; set; }

        public string Prefix { get; set; }

        public string Name { get; set; }

        public List<int> Personen { get; } = new List<int>();
    }
}
