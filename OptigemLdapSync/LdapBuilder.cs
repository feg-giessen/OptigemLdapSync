using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using OptigemLdapSync.Models;

namespace OptigemLdapSync
{
    internal static class LdapBuilder
    {
        private const string LdapDateTimeFormat = @"yyyyMMddHHmmss'Z'";

        public static readonly string[] AllAttributes = { "dn", "objectclass", "syncuserid", "syncusersource", "userpassword", "cn", "displayname", "givenname", "sn", "title", "gender", "street", "l", "postalcode", "c", "telephonenumber", "facsimiletelephonenumber", "mail", "dateofbirth", "startdate", "enddate", "typo3disabled" };

        public static readonly string[] CreateAttributes = { "objectclass", "syncuserid", "syncusersource", "userpassword" };

        public static IEnumerable<DirectoryAttribute> GetAllAttributes(PersonModel model, bool disabled)
        {
            return GetCreateAttributes(model)
                .Concat(GetUpdateAttributes(model, disabled));
        }

        public static IEnumerable<DirectoryAttribute> GetCreateAttributes(PersonModel model)
        {
            yield return new DirectoryAttribute("objectclass", "top", "person", "organizationalPerson", "inetOrgPerson", "fegperson", "simpleSecurityObject");

            yield return new DirectoryAttribute("syncuserid", model.SyncUserId.ToString());
            yield return new DirectoryAttribute("syncusersource", "fe_users");
            yield return new DirectoryAttribute("userpassword", GenerateSaltedSha1(model.Password));
        }

        public static IEnumerable<DirectoryAttribute> GetUpdateAttributes(PersonModel model, bool disabled)
        {
            yield return new DirectoryAttribute("cn", GetCn(model.Username));

            if (!string.IsNullOrWhiteSpace(model.Titel))
            {
                // Namenszusätze (von, zu, de, ...) sind auch im titel Feld gespeichert.
                // Diese könnne auch mit echten Titeln kombiniert sein (z.B. "Dr. von Mustermann").
                // Daher filtern wird die Titel hier raus und verwenden dann nur noch die echten Namenszusätze.
                string titel = Regex.Replace(model.Titel, @"\w+\.\s*", string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(titel))
                    yield return new DirectoryAttribute("displayname", (titel + " " + model.Nachname?.Trim() + ", " + model.Vorname).Trim().Trim(','));
                else
                    yield return new DirectoryAttribute("displayname", (model.Nachname + ", " + model.Vorname).Trim().Trim(','));
            }
            else
                yield return new DirectoryAttribute("displayname", (model.Nachname + ", " + model.Vorname).Trim().Trim(','));
            yield return new DirectoryAttribute("givenname", model.Vorname);
            yield return new DirectoryAttribute("sn", model.Nachname);

            if (!string.IsNullOrWhiteSpace(model.Titel))
                yield return new DirectoryAttribute("title", model.Titel);

            yield return new DirectoryAttribute("gender", ((int)model.Geschlecht).ToString());

            if (!model.NoAddress)
            {
                if (!string.IsNullOrWhiteSpace(model.Strasse))
                    yield return new DirectoryAttribute("street", model.Strasse);

                if (!string.IsNullOrWhiteSpace(model.Ort))
                    yield return new DirectoryAttribute("l", model.Ort);

                if (!string.IsNullOrWhiteSpace(model.Plz))
                    yield return new DirectoryAttribute("postalcode", model.Plz);

                if (!string.IsNullOrWhiteSpace(model.Land))
                    yield return new DirectoryAttribute("c", model.Land.SanitizeSingleLine());
            }

            if (!string.IsNullOrWhiteSpace(model.Telefon))
                yield return new DirectoryAttribute("telephonenumber", model.Telefon.SanitizeSingleLine());

            if (!string.IsNullOrWhiteSpace(model.Telefax))
                yield return new DirectoryAttribute("facsimiletelephonenumber", model.Telefax.SanitizeSingleLine());

            if (!string.IsNullOrWhiteSpace(model.EMail))
                yield return new DirectoryAttribute("mail", model.EMail);

            if (model.Geburtsdatum.HasValue)
            {
                yield return new DirectoryAttribute("dateofbirth", model.Geburtsdatum.Value.ToString("yyyyMMdd"));
            }

            if (model.StartDatum.HasValue)
            {
                yield return new DirectoryAttribute("startdate", model.StartDatum.Value.Date.ToString(LdapDateTimeFormat));
            }

            if (model.EndDatum.HasValue && (!model.StartDatum.HasValue || model.EndDatum.Value > model.StartDatum.Value))
            {
                yield return new DirectoryAttribute("enddate", model.EndDatum.Value.Date.ToString(LdapDateTimeFormat));
            }

            yield return new DirectoryAttribute("typo3disabled", disabled ? "TRUE" : "FALSE");
        }

        public static IEnumerable<DirectoryAttributeModification> GetDiff(IEnumerable<DirectoryAttribute> latest, SearchResultEntry existing, string[] omittedAttributes = null)
        {
            var existingList = existing.Attributes.Values?.OfType<DirectoryAttribute>().ToList() ?? new List<DirectoryAttribute>();

            foreach (DirectoryAttribute item in latest)
            {
                if (omittedAttributes != null && omittedAttributes.Contains(item.Name, StringComparer.InvariantCultureIgnoreCase))
                    continue;

                DirectoryAttribute existingItem = existingList.FirstOrDefault(e => string.Equals(e.Name, item.Name, StringComparison.InvariantCultureIgnoreCase));

                if (existingItem == null)
                {
                    yield return item.CreateModification(DirectoryAttributeOperation.Add);
                }
                else
                {
                    string[] existingValues = existingItem.GetValues<string>();
                    Debug.Assert(existingValues.Length == existingItem.Count);

                    string[] newValues = item.GetValues<string>();
                    Debug.Assert(newValues.Length == item.Count);

                    if (newValues.NonIntersect(existingValues).Any())
                    {
                        yield return item.CreateModification(DirectoryAttributeOperation.Replace);
                    }

                    existingList.Remove(existingItem);
                }
            }

            // Remove old attributes which are no longer filled.
            foreach (DirectoryAttribute item in existingList)
            {
                if (omittedAttributes != null && omittedAttributes.Contains(item.Name, StringComparer.InvariantCultureIgnoreCase))
                    continue;

                yield return item.CreateModification(DirectoryAttributeOperation.Delete);
            }
        }

        public static string GetCn(string name)
        {
            if (name == null)
                return null;

            string[] parts = Regex.Split(name, @"\r|\r\n|\n");

            return string.Join(string.Empty, parts.Select(p => p.Trim().Replace(",", string.Empty).Replace("  ", " ")));
        }

        private static string SanitizeSingleLine(this string input)
        {
            if (input == null)
                return null;

            string[] parts = Regex.Split(input, @"\r|\r\n|\n");
            return parts.Length > 0 ? parts[0] : input;
        }

        public static string GenerateSaltedSha1(this string plainTextString)
        {
            HashAlgorithm algorithm = new SHA1Managed();
            byte[] saltBytes = GenerateSalt(10);
            byte[] plainTextBytes = Encoding.ASCII.GetBytes(plainTextString);

            byte[] plainTextWithSaltBytes = AppendByteArray(plainTextBytes, saltBytes);
            byte[] saltedSha1Bytes = algorithm.ComputeHash(plainTextWithSaltBytes);
            byte[] saltedSha1WithAppendedSaltBytes = AppendByteArray(saltedSha1Bytes, saltBytes);

            return "{SSHA}" + Convert.ToBase64String(saltedSha1WithAppendedSaltBytes);
        }

        private static byte[] GenerateSalt(int saltSize)
        {
            var rng = new RNGCryptoServiceProvider();

            byte[] buffer = new byte[saltSize];
            rng.GetBytes(buffer);

            return buffer;
        }

        private static byte[] AppendByteArray(byte[] byteArray1, byte[] byteArray2)
        {
            var result = new byte[byteArray1.Length + byteArray2.Length];

            Array.Copy(byteArray1, 0, result, 0, byteArray1.Length);
            Array.Copy(byteArray2, 0, result, byteArray1.Length, byteArray2.Length);

            return result;
        }
    }
}
