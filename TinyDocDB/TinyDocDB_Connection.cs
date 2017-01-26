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

        private TinyDocDB_Resource StartMonitoringResource(string path, string resourceType, string resourceName, int pollingRate)
        {
            string keyName = path + "_" + resourceName;
            try
            { 
                monitoredResources.Add(keyName, new TinyDocDB_Resource(uri, key, path, resourceType, resourceName, pollingRate));
            }
            catch (ArgumentException ex) // and ArgumentNullException
            {
                throw new TinyDocDB_Exception("An invalid resource value was passed to be monitored", ex);
            }
            return monitoredResources[keyName];
        }

        private string GetResourceResult(string verb, string uri, string key, string queryPath, string resourceType = "", string resourceValue = "")
        {
            Task<string> resourceTask = TinyDocDB_HttpRequestHelper.PerformResourceRequest(verb, uri, key, queryPath, resourceType, resourceValue);
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

        public TinyDocDB_Resource StartMonitoringDocument(string databaseName, string collectionName, string documentName)
        {
            return StartMonitoringDocument(databaseName, collectionName, documentName, defaultPollingRate);
        }

        public TinyDocDB_Resource StartMonitoringDocument(string databaseName, string collectionName, string documentName, int pollingIntervalMS)
        {
            string queryPath = "dbs/" + databaseName + "/colls/" + collectionName + "/docs/" + documentName;
            return StartMonitoringResource(queryPath, "docs", documentName, pollingIntervalMS);
        }

        public string GetDocument(string databaseName, string collectionName, string documentName)
        {
            string queryPath = "dbs/" + databaseName + "/colls/" + collectionName + "/docs/" + documentName;
            return GetResourceResult("get", uri, key, queryPath, "docs", documentName);
        }

        public string UpdateDocument(string databaseName, string collectionName, string documentName, string jsonDocument)
        {
            string queryPath = "dbs/" + databaseName + "/colls/" + collectionName + "/docs/" + documentName;
            return GetResourceResult("put", uri, key, queryPath, "docs", jsonDocument);
        }

        public string GetOffer(string offerId)
        {
            string queryPath = "offers/" + offerId;
            return GetResourceResult("get", uri, key, queryPath, "offers", offerId);
        }
        public string GetOffers()
        {
            string queryPath = "offers";
            return GetResourceResult("get", uri, key, queryPath);
        }

        public string PutOffer(string offerId, string newOfferJson)
        {
            string queryPath = "offers/" + offerId;
            return GetResourceResult("put", uri, key, queryPath, "offers", newOfferJson);
        }
    }
}
