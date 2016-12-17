namespace OptigemLdapSync
{
    public class SyncConfiguration : ISyncConfiguration
    {
        public bool WhatIf { get; } = false;

        public string LdapDirectoryBaseDn { get; } = "dc=feg-giessen,dc=de";

        public string LdapBenutzerBaseDn => "ou=benutzer," + this.LdapDirectoryBaseDn;

        public string LdapInaktiveBenutzerBaseDn => "ou=inaktiv," + this.LdapDirectoryBaseDn;

        public string LdapGruppenBaseDn => "ou=gruppen," + this.LdapDirectoryBaseDn;

        public string LdapSyncGroupSource { get; } = "og_categories";

        public string OptigemDatabasePath { get; } = @"C:\temp\optigem";

        public string[] LdapDefaultParentGroups { get; } = { "cn=ownCloud,ou=gruppen,dc=feg-giessen,dc=de" };
    }
}
