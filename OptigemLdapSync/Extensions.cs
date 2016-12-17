using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;

namespace OptigemLdapSync
{
    internal static class Extensions
    {
        public static string ExtractCn(this string dn)
        {
            return dn.Split(new[] { ',' }, 2)
                .First()
                .Replace("cn=", string.Empty);
        }

        public static string ExtractBaseDn(this string dn)
        {
            return dn.Split(new[] { ',' }, 2)
                .Skip(1)
                .First();
        }

        public static T[] GetValues<T>(this DirectoryAttribute item) 
            where T : class
        {
            return (T[])item.GetValues(typeof(T));
        }

        public static IEnumerable<T> NonIntersect<T>(this IEnumerable<T> self, IEnumerable<T> other)
        {
            if (self == null)
                return other;

            if (other == null)
                return self;

            return self.Except(other)
                .Union(other.Except(self));
        }

        public static DirectoryAttributeModification CreateModification(this DirectoryAttribute item, DirectoryAttributeOperation operation)
        {
            var result = new DirectoryAttributeModification
            {
                Name = item.Name,
                Operation = operation
            };

            if (operation != DirectoryAttributeOperation.Delete)
            {
                result.AddRange(item.GetValues(typeof(byte[])));
            }

            return result;
        }
    }
}
