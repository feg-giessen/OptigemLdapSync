using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Text;

namespace OptigemLdapSync
{
    internal static class Log
    {
        public static TraceSource Source { get; } = new TraceSource("OptigemLdapSync");

        public static IEnumerable<string> Print(DirectoryAttributeModification attribute)
        {
            int count = 0;
            foreach (string value in attribute.GetValues<string>())
            {
                count++;
                yield return $"{attribute.Operation}: Name={attribute.Name}, Value={value}";
            }

            if (count != attribute.Count)
            {
                foreach (string value in attribute.GetValues<byte[]>().Select(s => Encoding.UTF8.GetString(s)))
                {
                    yield return $"{attribute.Operation}: Name={attribute.Name}, Value={value}";
                }
            }
        }
    }
}
