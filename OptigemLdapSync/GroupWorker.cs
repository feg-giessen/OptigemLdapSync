using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using OptigemLdapSync.Models;

namespace OptigemLdapSync
{
    internal class GroupWorker
    {
        private readonly ISyncConfiguration configuration;

        private readonly LdapConnector ldap;

        private readonly OptigemConnector optigem;

        private IList<LdapGroup> groups;

        private List<PersonKategorieZuordnung> zuordnungen;

        private List<PersonenkategorieModel> categories;

        public GroupWorker(ISyncConfiguration configuration, LdapConnector ldap, OptigemConnector optigem)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (ldap == null)
                throw new ArgumentNullException(nameof(ldap));
            if (optigem == null)
                throw new ArgumentNullException(nameof(optigem));

            this.configuration = configuration;
            this.ldap = ldap;
            this.optigem = optigem;
        }

        private List<LdapGroup> GetLdapGroups()
        {
            return this.ldap.PagedSearch("(objectClass=groupOfNames)", this.configuration.LdapGruppenBaseDn, new[] { "cn", "syncgroupid", "syncgroupsource", "member" })
                .SelectMany(s => s.OfType<SearchResultEntry>())
                .Select(g => new LdapGroup(g))
                .ToList();
        }

        public void SyncMetadata(ITaskReporter reporter)
        {
            this.categories = this.optigem.GetCategories().ToList();
            Log.Source.TraceEvent(TraceEventType.Verbose, 0, "Fetched {0} groups from Optigem.", categories.Count);

            this.zuordnungen = this.optigem.GetAllKategorieZuordnungen().ToList();
            
            reporter.StartTask("Personenkategorien abgleichen", categories.Count + 1);

            this.groups = this.GetLdapGroups();
            Log.Source.TraceEvent(TraceEventType.Verbose, 0, "Fetched {0} groups from LDAP.", this.groups.Count);

            var openGroups = groups.Where(g => g.SyncGroupSource == this.configuration.LdapSyncGroupSource).ToList();
            Log.Source.TraceEvent(TraceEventType.Verbose, 0, "{0} groups from LDAP have correct sync source ('{1}').", openGroups.Count, this.configuration.LdapSyncGroupSource);

            foreach (PersonenkategorieModel category in categories)
            {
                reporter.Progress(category.Name);

                var group = groups.FirstOrDefault(g => g.SyncGroupSource == this.configuration.LdapSyncGroupSource && g.SyncGroupId == category.Id);
                
                string name = LdapBuilder.GetCn(category.Name);
                string groupDn = $"cn={name},{this.configuration.LdapGruppenBaseDn}";

                if (group != null)
                {
                    if (category.Name != name)
                    {
                        this.ldap.MoveEntry($"cn={group.Name},{this.configuration.LdapGruppenBaseDn}", this.configuration.LdapGruppenBaseDn, $"cn={name}");

                        Log.Source.TraceEvent(TraceEventType.Information, 0, "Updated group name from '{0}' to '{1}'.", group.Name, name);
                        reporter.Log($"Updated group name from '{group.Name}' to '{name}'.");

                        group.Name = name;
                    }

                    openGroups.Remove(group);
                }
                else
                {
                    // Add new group
                    this.ldap.AddEntry(
                        groupDn,
                        new[]
                        {
                            new DirectoryAttribute("cn", name),
                            new DirectoryAttribute("syncgroupsource", this.configuration.LdapSyncGroupSource),
                            new DirectoryAttribute("syncgroupid", category.Id.ToString()),
                            new DirectoryAttribute("objectclass", "top", "groupOfNames", "feggroup")
                        });

                    Log.Source.TraceEvent(TraceEventType.Information, 0, "Added new group '{0}'.", groupDn);
                    reporter.Log($"Added new group '{groupDn}'.");

                    if (this.configuration.LdapDefaultParentGroups != null)
                    {
                        foreach (string parentGroupDn in this.configuration.LdapDefaultParentGroups)
                        {
                            var addAttribute = new DirectoryAttributeModification { Name = "member", Operation = DirectoryAttributeOperation.Add };
                            addAttribute.Add(groupDn);

                            this.ldap.ModifyEntry(
                                parentGroupDn,
                                new[] { addAttribute });

                            Log.Source.TraceEvent(TraceEventType.Information, 0, "Added new group '{0}' to parent group '{1}'.", name, parentGroupDn);
                            reporter.Log($"Added new group '{name}' to parent group '{parentGroupDn}'.");
                        }
                    }

                    groups.Add(new LdapGroup { Name = name, SyncGroupId = category.Id, SyncGroupSource = this.configuration.LdapSyncGroupSource });
                }
            }

            reporter.Progress("Checking for obsolete groups...");

            // Remove obsolete groups
            foreach (var group in openGroups)
            {
                this.ldap.DeleteEntry($"cn={group.Name},{this.configuration.LdapGruppenBaseDn}");
                Log.Source.TraceEvent(TraceEventType.Information, 0, "Deleted obsolete group '{0}'.", group.Name);
                reporter.Log($"Deleted obsolete group '{group.Name}'.");
                groups.Remove(group);
            }
        }

        public void SyncMembership(ITaskReporter reporter)
        {
            var filteredGroups = this.groups.Where(g => g.SyncGroupSource == this.configuration.LdapSyncGroupSource).ToList();
            reporter.StartTask("Gruppenmitgliedschaften abgleichen", filteredGroups.Count);

            // Refetch group members (might have been changed/renamed!).
            var newGroups = this.GetLdapGroups();

            foreach (var group in filteredGroups)
            {
                reporter.Progress(group.Name);

                // Set base for diff with new values.
                LdapGroup newGroup = newGroups.FirstOrDefault(g => g.SyncGroupId == group.SyncGroupId && g.SyncGroupSource == this.configuration.LdapSyncGroupSource);
                if (newGroup != null)
                {
                    group.SetOriginalMembers(newGroup.OrginalMembers);
                }

                DirectoryAttributeModification[] addedMembers = group.AddedMembers
                    .Select(m => new DirectoryAttribute("member", m).CreateModification(DirectoryAttributeOperation.Add))
                    .ToArray();

                DirectoryAttributeModification[] removedMembers = group.RemovedMembers
                    .Where(m => !m.EndsWith(this.configuration.LdapGruppenBaseDn))
                    .Select(m => new DirectoryAttribute("member", m).CreateModification(DirectoryAttributeOperation.Delete, forceValue: true))
                    .ToArray();

                if (addedMembers.Any() || removedMembers.Any())
                {
                    string groupDn = $"cn={group.Name},{this.configuration.LdapGruppenBaseDn}";
                    DirectoryAttributeModification[] modifications = addedMembers.Concat(removedMembers).ToArray();

                    this.ldap.ModifyEntry(groupDn, modifications);

                    foreach (var item in modifications)
                    {
                        foreach (string name in item.GetValues<string>())
                        {
                            reporter.Log((item.Operation == DirectoryAttributeOperation.Add ? "Hinzugefügt" : "Entfernt") + ": " + name);
                        }
                    }

                    Log.Source.TraceEvent(TraceEventType.Information, 0, "Updated members of LDAP group '{0}': {1} added, {2} removed.", group.Name, addedMembers.Length, removedMembers.Length);
                    foreach (string modifiedPrint in modifications.SelectMany(Log.Print))
                    {
                        Log.Source.TraceEvent(TraceEventType.Verbose, 0, "Update to LDAP group '{0}': {1}.", group.Name, modifiedPrint);
                    }
                }
            }
        }

        public LdapGroup Get(PersonenkategorieModel optigemCategory)
        {
            if (optigemCategory == null)
                return null;

            return this.groups.FirstOrDefault(g => g.SyncGroupSource == this.configuration.LdapSyncGroupSource && g.SyncGroupId == optigemCategory.Id);
        }

        public ICollection<PersonenkategorieModel> GetKategorien(int personId)
        {
            return this.zuordnungen
                .Where(z => z.PersonId == personId)
                .Select(z => this.categories.FirstOrDefault(c => c.Id == z.KategorieId))
                .Where(c => c != null)
                .ToList();
        }
    }
}
