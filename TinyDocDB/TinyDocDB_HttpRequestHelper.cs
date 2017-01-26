using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace TinyDocDB
{
    internal static class TinyDocDB_HttpRequestHelper
    {
        private static HttpClient client = new HttpClient();

        internal async static Task<string> PerformResourceRequest(string verb, string url, string key, string path, string resourceType, string body)
        {
            string response = null;

            try
            {
                Uri uri = new Uri(new Uri(url), path);
                
                string utcDate = DateTime.UtcNow.ToString("r");
                string authHeader = TinyDocDB_Helpers.GenerateMasterKeyAuthorizationSignature(utcDate, verb, path, resourceType, key, "master", "1.0");
                using (HttpRequestMessage requestMessage = new HttpRequestMessage())
                {
                    requestMessage.Headers.Add("authorization", authHeader);
                    requestMessage.Headers.Add("x-ms-date", utcDate);
                    requestMessage.Headers.Add("x-ms-version", "2015-12-16");
                    requestMessage.Headers.Add("Accept", "application/json");
                    requestMessage.RequestUri = uri;
                    switch (verb)
                    {
                        case "get":
                            requestMessage.Method = new HttpMethod("GET");
                            break;
                        case "put":
                            requestMessage.Method = new HttpMethod("PUT");
                            StringContent stringContent = new StringContent(body);
                            requestMessage.Content = stringContent;
                            break;
                        default:
                            throw new TinyDocDB_Exception("Unknown VERB: " + verb + ", recognized verbs are: get and put");
                    }
                    HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
                    HttpContent httpContent = responseMessage.Content;
                    response = await httpContent.ReadAsStringAsync();
                }
            }
            catch (FormatException ex) // and UriFormatException
            {
                throw new TinyDocDB_Exception("An exception occured getting the resource from DocumentDB, resourceId path = " + path, ex);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new TinyDocDB_Exception("An exception occured getting the resource from DocumentDB, resourceId path = " + path, ex);
            }
            catch (ArgumentNullException ex)
            {
                throw new TinyDocDB_Exception("An exception occured getting the resource from DocumentDB, resourceId path = " + path, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new TinyDocDB_Exception("An exception occured getting the resource from DocumentDB, resource path = " + path, ex);
            }
            return response;
        }
    }
}
