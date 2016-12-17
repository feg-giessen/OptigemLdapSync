using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Text;

namespace OptigemLdapSync.Models
{
    [DebuggerDisplay("{Name} {SyncGroupSource} ({SyncGroupId}) Member: {MemberList.Count}")]
    public class LdapGroup
    {
        private readonly ICollection<string> originalMembers;

        public LdapGroup()
        {
        }

        public LdapGroup(SearchResultEntry entry)
        {
            this.Name = entry.Attributes["cn"][0].ToString();

            if (entry.Attributes.Contains("syncgroupid"))
            {
                this.SyncGroupId = Convert.ToInt32(entry.Attributes["syncgroupid"][0]);
            }

            if (entry.Attributes.Contains("syncgroupsource"))
            {
                this.SyncGroupSource = entry.Attributes["syncgroupsource"][0].ToString();
            }

            if (entry.Attributes.Contains("member"))
            {
                this.MemberList.AddRange(entry.Attributes["member"].OfType<byte[]>().Select(m => Encoding.UTF8.GetString(m)));
                this.MemberList.AddRange(entry.Attributes["member"].OfType<string>());
            }

            this.originalMembers = new ReadOnlyCollection<string>(this.MemberList);
            this.MemberList.Clear();
        }

        public string Name { get; set; }

        public int? SyncGroupId { get; set; }

        public string SyncGroupSource { get; set; }

        public List<string> MemberList { get; } = new List<string>();

        public IEnumerable<string> AddedMembers => this.MemberList.Where(m => !this.originalMembers.Contains(m));

        public IEnumerable<string> RemovedMembers => this.originalMembers.Where(m => !this.MemberList.Contains(m));
    }
}
