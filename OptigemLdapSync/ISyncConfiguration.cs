namespace OptigemLdapSync
{
    public interface ISyncConfiguration
    {
        bool WhatIf { get; }

        string LdapDirectoryBaseDn { get; }

        string LdapBenutzerBaseDn { get; }

        string LdapInaktiveBenutzerBaseDn { get; }

        string LdapGruppenBaseDn { get; }

        string LdapSyncGroupSource { get; }

        string OptigemDatabasePath { get; }

        string[] LdapDefaultParentGroups { get; }
    }
}