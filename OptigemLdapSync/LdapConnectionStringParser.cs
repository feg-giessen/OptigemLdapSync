using System;
using System.Data.Common;
using System.Globalization;

namespace OptigemLdapSync
{
    internal class LdapConnectionStringParser : DbConnectionStringBuilder
    {
        public string Server
        {
            get { return (string)this["Server"]; }
            set { this["Server"] = value; }
        }

        public int Port
        {
            get { return Convert.ToInt32(this["Port"]); }
            set { this["Port"] = value.ToString(CultureInfo.InvariantCulture); }
        }

        public string User
        {
            get { return (string)this["User"]; }
            set { this["User"] = value; }
        }

        public string Password
        {
            get { return (string)this["Password"]; }
            set { this["Password"] = value; }
        }
    }
}
