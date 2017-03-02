using System;
using System.Text;

namespace TinyDocDB
{
    internal static class TinyDocDB_Helpers
    {
        internal static string GenerateMasterKeyAuthorizationSignature(string utcDate, string verb, string resourceType, string resourceValue, string key, string keyType, string tokenVersion)
        {
            try
            {
                var hmacSha256 = new System.Security.Cryptography.HMACSHA256 { Key = Convert.FromBase64String(key) };
                string payLoad = verb.ToLowerInvariant() + "\n" + 
                        (!String.IsNullOrWhiteSpace(resourceType) ? resourceType.ToLowerInvariant() : "") + "\n" +
                        (!String.IsNullOrWhiteSpace(resourceValue) ? resourceValue : "")  + "\n" +
                        utcDate.ToLowerInvariant() + "\n\n";

                byte[] hashPayLoad = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(payLoad));
                string signature = Convert.ToBase64String(hashPayLoad);
                return Uri.EscapeDataString(
                    String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "type=" + keyType + "&ver=" + tokenVersion + "&sig=" + signature));
            }
            catch(FormatException ex) // and UriFormatException
            {
                throw new TinyDocDB_Exception("Error Generating the Authentication Signature.", ex);
            }
            catch(EncoderFallbackException ex)
            {
                throw new TinyDocDB_Exception("Error Generating the Authentication Signature.", ex);
            }
            catch(ObjectDisposedException ex)
            {
                throw new TinyDocDB_Exception("Error Generating the Authentication Signature.", ex);
            }
            catch(ArgumentNullException ex)
            {
                throw new TinyDocDB_Exception("Error Generating the Authentication Signature.", ex);
            }
        }
    }
}
