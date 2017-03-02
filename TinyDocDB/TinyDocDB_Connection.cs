using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TinyDocDB
{
    public class TinyDocDB_Connection
    {
        public int defaultPollingRate { get; set; }
        private string uri = String.Empty;
        private string key = String.Empty;

        Dictionary<string, TinyDocDB_Resource> monitoredResources = new Dictionary<string, TinyDocDB_Resource>();

        public TinyDocDB_Connection(string serviceURI, string masterKey)
        {
            uri = serviceURI;
            key = masterKey;
            defaultPollingRate = 1000;
        }

        private TinyDocDB_Resource StartMonitoringResource(string queryPath, string resourceType, string resourceName, int pollingRate)
        {
            string keyName = queryPath + "_" + resourceName;
            try
            {
                monitoredResources.Add(keyName, new TinyDocDB_Resource(uri, key, queryPath, resourceType, resourceName, pollingRate));
            }
            catch (ArgumentException ex) // and ArgumentNullException
            {
                throw new TinyDocDB_Exception("An invalid resource value was passed to be monitored", ex);
            }
            return monitoredResources[keyName];
        }

        private string GetResourceResult(string verb, string uri, string key, string queryPath, string resourceType = "", string resourceValue = "", string body = "", bool jsonQuery = false)
        {
            Task<string> resourceTask = TinyDocDB_HttpRequestHelper.PerformResourceRequest(verb, uri, key, queryPath, resourceType, resourceValue, body, jsonQuery);
            try
            {
                resourceTask.Wait();
            }
            catch (ObjectDisposedException ex)
            {
                throw new TinyDocDB_Exception("Resource task had already completed or been destroyed", ex);
            }
            catch (AggregateException ex)
            {
                throw new TinyDocDB_Exception("Errors occured updating the resource", ex);
            }
            return resourceTask.Result;
        }

        public string GetAllDatabases()
        {
            string queryPath = "dbs/";
            return GetResourceResult("get", uri, key, queryPath, "dbs", "");
        }

        public void CreateDatabase(string databaseId)
        {
            string queryPath = "dbs/";
            string body = String.Format("{{\n   \"id\": \"{0}\" \n}}", databaseId);
            GetResourceResult("post", uri, key, queryPath, "dbs", "", body);
        }
 
        public string GetDatabase(string databaseId)
        {
            string queryPath = "dbs/" + databaseId;
            return GetResourceResult("get", uri, key, queryPath, "dbs", queryPath);
        }

        public string GetAllCollections(string databaseId)
        {
            string queryPath = String.Format("dbs/{0}/colls", databaseId);
            string resourceValue = queryPath.Substring(0, queryPath.LastIndexOf('/'));
            return GetResourceResult("get", uri, key, queryPath, "colls", resourceValue);
        }

        public void CreateCollection(string databaseId, string collectionId)
        {
            string queryPath = String.Format("dbs/{0}/colls", databaseId);
            string resourceValue = queryPath.Substring(0,queryPath.LastIndexOf('/'));
            string body = String.Format("{{\n   \"id\": \"{0}\" \n}}", collectionId);
            GetResourceResult("post", uri, key, queryPath, "colls", resourceValue, body);
        }

        public string GetCollection(string databaseId, string collectionId)
        {
            string queryPath = String.Format("dbs/{0}/colls/{1}", databaseId, collectionId);
            return GetResourceResult("get", uri, key, queryPath, "colls", queryPath);
        }

        public string CreateDocument(string databaseId, string collectionId, string jsonDocument)
        {
            string queryPath = String.Format("dbs/{0}/colls/{1}/docs", databaseId, collectionId);
            string resourceValue = queryPath.Substring(0, queryPath.LastIndexOf('/'));
            return GetResourceResult("post", uri, key, queryPath, "docs", resourceValue, jsonDocument);
        }
        public string GetDocument(string databaseId, string collectionId, string documentId)
        {
            string queryPath = String.Format("dbs/{0}/colls/{1}/docs/{2}", databaseId, collectionId, documentId);
            return GetResourceResult("get", uri, key, queryPath, "docs", queryPath);
        }

        public string UpdateDocument(string databaseId, string collectionId, string documentId, string jsonDocument)
        {
            string queryPath = String.Format("dbs/{0}/colls/{1}/docs/{2}", databaseId, collectionId, documentId);
            string resourceValue = queryPath.Substring(0, queryPath.LastIndexOf('/'));
            return GetResourceResult("put", uri, key, queryPath, "docs", queryPath, jsonDocument);
        }

        public string GetAllDocuments(string databaseId, string collectionId)
        {
            string queryPath = String.Format("dbs/{0}/colls/{1}/docs", databaseId, collectionId);
            string resourceValue = queryPath.Substring(0, queryPath.LastIndexOf('/'));
            return GetResourceResult("get", uri, key, queryPath, "docs", resourceValue);
        }

        public string QueryCollection(string databaseId, string collectionId, string jsonQuery)
        {
            string queryPath = String.Format("dbs/{0}/colls/{1}/docs", databaseId, collectionId);
            string resourceValue = queryPath.Substring(0, queryPath.LastIndexOf('/'));
            return GetResourceResult("post", uri, key, queryPath, "docs", resourceValue, jsonQuery, true);
        }
        public string DeleteDocument(string databaseId, string collectionId, string documentId)
        {
            string queryPath = String.Format("dbs/{0}/colls/{1}/docs/{2}", databaseId, collectionId, documentId);
            return GetResourceResult("delete", uri, key, queryPath, "docs", queryPath);
        }

        public string DeleteCollection(string databaseId, string collectionId)
        {
            string queryPath = String.Format("dbs/{0}/colls/{1}", databaseId, collectionId);
            return GetResourceResult("delete", uri, key, queryPath, "colls", queryPath);
        }

        public string DeleteDatabase(string databaseId)
        {
            string queryPath = String.Format("dbs/{0}", databaseId);
            return GetResourceResult("delete", uri, key, queryPath, "dbs", queryPath);
        }

        public TinyDocDB_Resource StartMonitoringDocument(string databaseId, string collectionId, string documentId)
        {
            return StartMonitoringDocument(databaseId, collectionId, documentId, defaultPollingRate);
        }

        public TinyDocDB_Resource StartMonitoringDocument(string databaseId, string collectionId, string documentId, int pollingIntervalMS)
        {
            string queryPath = String.Format("dbs/{0}/colls/{1}/docs/{2}", databaseId, collectionId, documentId);
            return StartMonitoringResource(queryPath, "docs", documentId, pollingIntervalMS);
        }
    }
}
