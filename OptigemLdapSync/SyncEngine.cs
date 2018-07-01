using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.IO;
using System.Linq;
using OptigemLdapSync.Models;

namespace OptigemLdapSync
{
    internal class SyncEngine
    {
        private readonly ISyncConfiguration configuration;

        private readonly OptigemConnector optigem;

        private readonly LdapConnector ldap;

        private readonly GroupWorker groups;

        private readonly DirectoryInfo logDir;

        public SyncEngine(ISyncConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            this.configuration = configuration;

            this.optigem = new OptigemConnector(this.configuration.OptigemDatabasePath);
            this.logDir = new DirectoryInfo(Path.Combine(this.configuration.OptigemDatabasePath, "syncLogs"));

            if (!this.logDir.Exists)
            {
                this.logDir.Create();
            }

            string ldapConnection = this.optigem.GetLdapConnectionString();

            if (string.IsNullOrEmpty(ldapConnection))
                throw new InvalidOperationException("No LDAP connection configured.");

            var parser = new LdapConnectionStringParser { ConnectionString = ldapConnection };
            this.ldap = new LdapConnector(parser.Server, parser.Port, AuthType.Basic, parser.User, parser.Password, 100, this.configuration.WhatIf);

            this.groups = new GroupWorker(this.configuration, this.ldap, this.optigem);
        }

        public SyncResult Do(ITaskReporter reporter, bool fullSync)
        {
            string logName = "SyncLog_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log";
            using (var wrappedReport = new LoggingReporter(Path.Combine(this.logDir.FullName, logName), reporter))
            {
                return this.InternalDo(wrappedReport, fullSync);
            }
        }

        private SyncResult InternalDo(ITaskReporter reporter, bool fullSync)
        {
            if (reporter == null)
                throw new ArgumentNullException(nameof(reporter));

            // 1: Group metadata
            // 2: Prepare new users
            // 3: Sync users
            // 4: Check orphans
            // 5: Syng group membership
            reporter.Init(fullSync ? 5 : 3);

            // Read LDAP groups and sync meta data.
            this.groups.SyncMetadata(reporter);

            // Clean-up database
            this.optigem.SetCustomFieldsToNull();

            var result = new SyncResult();

            if (fullSync)
            {
                // Create sync meta data in Optigem for new users.
                result.Created = this.CreateNewLdapUsers(reporter);
            }

            IList<PersonModel> persons = this.optigem.GetAllPersons()
                .ToList();
            
            reporter.StartTask("Personen abgleichen", persons.Count + 1);

            reporter.Progress("LDAP-Benutzer laden");
            List<SearchResultEntry> allPersonUsers = this.ldap.PagedSearch("(&(objectClass=fegperson))", this.configuration.LdapDirectoryBaseDn, LdapBuilder.AllAttributes) //new[] { "cn", "dn", "syncuserid" }
                .SelectMany(s => s.OfType<SearchResultEntry>())
                .ToList();

            foreach (PersonModel person in persons)
            {
                reporter.Progress(person.Username);

                var existingUser = allPersonUsers.FirstOrDefault(u => u.Attributes["syncuserid"][0]?.ToString() == person.SyncUserId.ToString());
                this.SyncUser(reporter, person, existingUser, fullSync);

                if (person.Aenderung && fullSync)
                {
                    this.optigem.SetChangedFlag(person.Nr, false);
                }

                if (existingUser != null)
                {
                    allPersonUsers.Remove(existingUser);
                }
            }

            if (fullSync)
            {
                reporter.StartTask("Offene LDAP-Benutzer abgleichen", allPersonUsers.Count);
                foreach (SearchResultEntry entry in allPersonUsers)
                {
                    reporter.Progress(entry.DistinguishedName);
                    reporter.Log("Manual action required.");
                }
            }

            this.groups.SyncMembership(reporter);

            return result;
        }

        private int CreateNewLdapUsers(ITaskReporter reporter)
        {
            IList<PersonModel> persons = this.optigem.GetNewPersons().ToList();
            reporter.StartTask("Neue Personen in OPTIGEM vorbereiten", persons.Count);

            if (persons.Any())
            {
                int intranetUid = this.optigem.GetNextSyncUserId();

                foreach (var person in persons)
                {
                    string username = LdapBuilder.GetCn((person.Vorname?.Trim() + "." + person.Nachname?.Trim()).Trim('.')).ToLower(CultureInfo.CurrentCulture);

                    reporter.Progress(username);

                    person.Username = username.Length > 50 ? username.Substring(0, 50) : username;
                    person.Password = this.CalculatePassword();
                    person.SyncUserId = intranetUid++;

                    reporter.Log(person.Username + " mit Id " + person.SyncUserId + " angelegt.");
                }

                this.optigem.SetIntranetUserIds(persons);

                return persons.Count;
            }

            return 0;
        }

        private void SyncUser(ITaskReporter reporter, PersonModel model, SearchResultEntry entry, bool fullSync)
        {
            ICollection<PersonenkategorieModel> kategorien = this.groups.GetKategorien(model.Nr);
                // this.optigem.GetPersonenkategorien(model.Nr).ToList();

            bool disabled = this.IsDisabled(model, kategorien);

            string baseDn;
            string requiredBaseDn;

            string mitgliederBase = "ou=mitglieder," + this.configuration.LdapBenutzerBaseDn;
            string externBase = "ou=extern," + this.configuration.LdapBenutzerBaseDn;

            if (disabled)
            {
                requiredBaseDn = this.configuration.LdapInaktiveBenutzerBaseDn;
                baseDn = requiredBaseDn;
            }
            else
            {
                requiredBaseDn = this.configuration.LdapBenutzerBaseDn;
                baseDn = kategorien.Any(k => k.Name == "Mitglied") ? mitgliederBase : externBase;
            }

            string cn = LdapBuilder.GetCn(model.Username);
            string dn = $"cn={cn},{baseDn}";

            if (disabled && model.EndDatum.HasValue && model.EndDatum.Value < DateTime.Today.AddYears(-2))
            {
                // More than two years inactive => delete in LDAP

                // No entry in LDAP => ok, NOP
                if (entry != null)
                {
                    reporter.Log($"Benutzer wird gelöscht (mehr als 2 Jahre inaktiv): {dn}");
                    this.ldap.DeleteEntry(dn);
                }

                return;
            }

            if (entry == null)
            {
                SearchResultEntry[] searchResult = this.ldap
                    .PagedSearch($"(&(objectClass=inetOrgPerson)(syncUserId={model.SyncUserId}))", this.configuration.LdapDirectoryBaseDn, LdapBuilder.AllAttributes)
                    .SelectMany(s => s.OfType<SearchResultEntry>())
                    .ToArray();

                if (searchResult.Length == 0)
                {
                    DirectoryAttribute[] attributes = LdapBuilder.GetAllAttributes(model, disabled).ToArray();

                    this.ldap.AddEntry(dn, attributes);
                    Log.Source.TraceEvent(TraceEventType.Information, 0, "Added new LDAP user '{0}'.", dn);
                    reporter.Log($"Neuer Benutzer hinzugefügt: {dn}");
                }
                else
                {
                    entry = searchResult.First();
                }
            }

            if (entry != null)
            {
                string oldDn = entry.DistinguishedName;
                string oldBaseDn = oldDn.Split(new[] { ',' }, 2).Last();

                Log.Source.TraceEvent(TraceEventType.Verbose, 0, "Syncing LDAP user '{0}'.", oldDn);

                if (!oldDn.EndsWith(requiredBaseDn)
                    || (oldBaseDn == externBase && baseDn == mitgliederBase)
                    || (oldBaseDn == mitgliederBase && baseDn == externBase))
                {
                    this.ldap.MoveEntry(oldDn, baseDn, $"cn={cn}");
                    Log.Source.TraceEvent(TraceEventType.Information, 0, "Moved LDAP user from '{0}' to '{1}'.", oldDn, dn);
                    reporter.Log($"Benutzer von {oldDn} nach {dn} verschoben.");
                }
                else
                {
                    // User was not moved. Set baseDn to actual baseDn.
                    baseDn = oldBaseDn;
                    dn = $"cn={cn},{baseDn}";

                    string oldCn = entry.Attributes["cn"][0]?.ToString();
                    if (oldCn != cn)
                    {
                        dn = $"cn={cn},{oldBaseDn}";
                        this.ldap.MoveEntry(oldDn, oldBaseDn, $"cn={cn}");
                        Log.Source.TraceEvent(TraceEventType.Information, 0, "Renamed LDAP user from '{0}' to '{1}'.", oldCn, cn);
                        reporter.Log($"Benutzer von {oldCn} nach {cn} umbenannt.");
                    }
                }

                DirectoryAttributeModification[] diffAttributes = null;

                if (fullSync)
                {
                    diffAttributes = LdapBuilder.GetDiff(
                            LdapBuilder.GetUpdateAttributes(model, disabled),
                            entry,
                            LdapBuilder.CreateAttributes.Union(new[] { "cn", "dn" }).ToArray())
                        .ToArray();
                }
                else
                {
                    // only update disabled attribute
                    diffAttributes = LdapBuilder.GetDiff(
                            LdapBuilder.GetUpdateAttributes(model, disabled),
                            entry,
                            LdapBuilder.AllAttributes.Except(new[] { "typo3disabled" }).ToArray())
                        .ToArray();
                }

                if (diffAttributes?.Any() ?? false)
                {
                    this.ldap.ModifyEntry(dn, diffAttributes);
                    Log.Source.TraceEvent(TraceEventType.Information, 0, "Updated LDAP user '{0}'.", dn);
                    foreach (var diff in diffAttributes)
                    {
                        var oldAttr = entry.Attributes.Values?.OfType<DirectoryAttribute>().FirstOrDefault(a => string.Equals(a.Name, diff.Name, StringComparison.InvariantCultureIgnoreCase));
                        string oldValue = oldAttr == null ? string.Empty : string.Join("', '", oldAttr.GetValues<string>());
                        Debug.Assert(oldValue != string.Join("', '", diff.GetValues<string>()));
                        reporter.Log($"{diff.Name} auf '{string.Join("', '", diff.GetValues<string>())}' gesetzt (alt: '{oldValue}').");
                    }

                    foreach (string attributeChange in diffAttributes.SelectMany(Log.Print))
                    {
                        Log.Source.TraceEvent(TraceEventType.Verbose, 0, "Updated LDAP user '{0}': {1}", cn, attributeChange);
                    }
                }
            }

            if (!disabled)
            {
                foreach (PersonenkategorieModel kategorie in kategorien)
                {
                    LdapGroup group = this.groups.Get(kategorie);

                    if (group == null)
                    {
                        Log.Source.TraceEvent(TraceEventType.Warning, 0, "LDAP group does not exist: '{0}'", kategorie.Name);
                    }
                    else
                    {
                        group.MemberList.Add(dn);
                    }
                }
            }
        }

        private bool IsDisabled(PersonModel model, ICollection<PersonenkategorieModel> kategorien)
        {
            bool mitglied = kategorien.Any(k => k.Name == "Mitglied");

            if (mitglied)
            {
                if (model.StartDatum.HasValue && model.StartDatum.Value > DateTime.Now)
                    return true;

                if (model.EndDatum.HasValue && model.EndDatum.Value < DateTime.Now
                    && (!model.StartDatum.HasValue || model.EndDatum.Value >= model.StartDatum.Value))
                    return true;
            }

            return !kategorien.Any();
        }

        private string CalculatePassword()
        {
            const int pinLaenge = 8;
            const string charList1 = "abcdefghijklmnopqrstuvwxyz";
            const string charList2 = "1234567890";

            char[] result = new char[pinLaenge];
            int i;

            Random rand = new Random(Environment.TickCount);

            for (i = 0; i < pinLaenge; i++)
            {
                int pos;
                string list = i < 4 ? charList1 : charList2;

                do
                {
                    pos = (int)(list.Length * rand.NextDouble());
                }
                while (pos < 0 || pos >= list.Length);

                result[i] = list[pos];
            }

            return new string(result);
        }
    }
}
