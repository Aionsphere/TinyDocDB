using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;

namespace TinyDocDB
{
    internal static class TinyDocDB_HttpRequestHelper
    {
        private static HttpClient client = new HttpClient();
     
        internal async static Task<string> PerformResourceRequest(string verb, string url, string key, string queryPath, string resourceType, string resourceValue, string body = "", bool isQuery = false)
        {
            string response = null;

            try
            {
                Uri uri = new Uri(new Uri(url), queryPath);
                
                string utcDate = DateTime.UtcNow.ToString("r");
                string authHeader = TinyDocDB_Helpers.GenerateMasterKeyAuthorizationSignature(utcDate, verb, resourceType, resourceValue, key, "master", "1.0");
                using (HttpRequestMessage requestMessage = new HttpRequestMessage())
                {
                    requestMessage.Headers.Add("authorization", authHeader);
                    requestMessage.Headers.Add("x-ms-date", utcDate);
                    requestMessage.Headers.Add("x-ms-version", "2015-12-16");
                    requestMessage.Headers.Add("Accept", "application/json");
                    if (isQuery)
                    {
                        
                        requestMessage.Headers.Add("x-ms-documentdb-isquery", "true");
                    }
                    requestMessage.RequestUri = uri;
                    switch (verb)
                    {
                        case "delete":
                            requestMessage.Method = new HttpMethod("DELETE");
                            break;
                        case "get":
                            requestMessage.Method = new HttpMethod("GET");
                            break;
                        case "put":
                            requestMessage.Method = new HttpMethod("PUT");
                            StringContent stringContent = new StringContent(body);
                            requestMessage.Content = stringContent;
                            break;
                        case "post":
                            requestMessage.Method = new HttpMethod("POST");
                            StringContent cont;
                            if (!isQuery)
                            {
                                cont = new StringContent(body);
                            }
                            else
                            {
                                cont = new StringContent(body, Encoding.ASCII, "application/query+json");
                                cont.Headers.ContentType.CharSet = "";
                            }
                            requestMessage.Content = cont;
                            break;
                        default:
                            throw new TinyDocDB_Exception("Unknown VERB: " + verb + ", recognized verbs are: get, post, delete and put");
                    }
                    HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
                    HttpContent httpContent = responseMessage.Content;
                    response = await httpContent.ReadAsStringAsync();
                }
            }
            catch (FormatException ex) // and UriFormatException
            {
                throw new TinyDocDB_Exception("An exception occured getting the resource from DocumentDB, resourceId path = " + queryPath, ex);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new TinyDocDB_Exception("An exception occured getting the resource from DocumentDB, resourceId path = " + queryPath, ex);
            }
            catch (ArgumentNullException ex)
            {
                throw new TinyDocDB_Exception("An exception occured getting the resource from DocumentDB, resourceId path = " + queryPath, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new TinyDocDB_Exception("An exception occured getting the resource from DocumentDB, resource path = " + queryPath, ex);
            }
            return response;
        }
    }
}
