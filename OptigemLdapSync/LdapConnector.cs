using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;

namespace OptigemLdapSync
{
    internal class LdapConnector
    {
        private readonly LdapConnection ldapConnection;
        
        private readonly int pageSize;

        private readonly bool whatIf;

        public LdapConnector(string hostName, int portNumber, AuthType authType, string connectionAccountName, string connectionAccountPassword, int pageSize, bool whatIf = false)
        {
            this.pageSize = pageSize;
            this.whatIf = whatIf;

            var ldapDirectoryIdentifier = new LdapDirectoryIdentifier(hostName, portNumber, true, false);

            var networkCredential = new NetworkCredential(connectionAccountName, connectionAccountPassword);

            this.ldapConnection = new LdapConnection(ldapDirectoryIdentifier, networkCredential, authType)
            {
                AutoBind = true
            };

            this.ldapConnection.SessionOptions.ProtocolVersion = 3;
            this.ldapConnection.SessionOptions.SecureSocketLayer = true;
            this.ldapConnection.SessionOptions.VerifyServerCertificate = (connection, certificate) => true;
        }

        public bool AddEntry(string dn, DirectoryAttribute[] attributes)
        {
            if (this.whatIf)
                return true;

            var request = new AddRequest(dn, attributes);
            DirectoryResponse response = this.ldapConnection.SendRequest(request);

            ////var addResponse = response as AddResponse;
            return response?.ResultCode == ResultCode.Success;
        }

        public bool DeleteEntry(string dn)
        {
            if (this.whatIf)
                return true;

            var request = new DeleteRequest(dn);
            DirectoryResponse response = this.ldapConnection.SendRequest(request);

            ////var addResponse = response as AddResponse;
            return response?.ResultCode == ResultCode.Success;
        }

        public bool ModifyEntry(string dn, DirectoryAttributeModification[] attributes)
        {
            if (this.whatIf)
                return true;

            var request = new ModifyRequest(dn, attributes);
            DirectoryResponse response = this.ldapConnection.SendRequest(request);

            ////var addResponse = response as AddResponse;
            return response?.ResultCode == ResultCode.Success;
        }

        public bool MoveEntry(string oldDn, string newBaseDn, string cn)
        {
            if (this.whatIf)
                return true;

            var request = new ModifyDNRequest(oldDn, newBaseDn, cn);
            DirectoryResponse response = this.ldapConnection.SendRequest(request);

            ////var addResponse = response as AddResponse;
            return response?.ResultCode == ResultCode.Success;
        }

        public IEnumerable<SearchResultEntryCollection> PagedSearch(string searchFilter, string baseDn, string[] attributesToLoad)
        {
            var searchRequest = new SearchRequest(baseDn, searchFilter, SearchScope.Subtree, attributesToLoad);

            var pageResultRequestControl = new PageResultRequestControl(this.pageSize);
            searchRequest.Controls.Add(pageResultRequestControl);

            while (true)
            {
                var searchResponse = (SearchResponse)this.ldapConnection.SendRequest(searchRequest);

                if (searchResponse == null)
                    break;

                var pageResponse = (PageResultResponseControl)searchResponse.Controls[0];

                yield return searchResponse.Entries;

                if (pageResponse.Cookie.Length == 0)
                    break;

                pageResultRequestControl.Cookie = pageResponse.Cookie;
            }
        }
    }
}
