using System;
using System.Threading.Tasks;
using System.Timers;

namespace TinyDocDB
{
    public class TinyDocDB_Resource
    {
        private string cachedResponse = "";
        private string rURI = "";
        private string rKey = "";
        private string rPath = "";
        private string rTypeName = "";
        private string rName = ""; 
        private int pollInterval = 1000;
        public event EventHandler<TinyDocDB_UpdateEventArgs> ResourceUpdate;
        Timer resourceUpdate;

        protected virtual void OnResourceUpdate(TinyDocDB_UpdateEventArgs e)
        {
            ResourceUpdate?.Invoke(this, e);
        }

        internal TinyDocDB_Resource(string uri, string key, string path, string resourceType, string resourceName, int pollingRate)
        {
            pollInterval = pollingRate;
            rURI = uri;
            rKey = key;
            rPath = path;
            rTypeName = resourceType;
            rName = resourceName;
        }

        public void Start()
        {
            if (this.ResourceUpdate == null)
            {
                throw new TinyDocDB_Exception("You must attach an event handler prior to calling Start otherwise we won't receive document updates");
            }

            try
            {
                resourceUpdate = new Timer(pollInterval);
                resourceUpdate.Elapsed += ResourceUpdate_Elapsed;
                resourceUpdate.Start();
            }
            catch(ArgumentOutOfRangeException ex)
            {
                throw new TinyDocDB_Exception("Invalid polling interval specified (Resource Update Timer)", ex);
            }
            catch(ArgumentException ex)
            {
                throw new TinyDocDB_Exception("Invalid polling interval specified (Resource Update Timer)", ex);
            }
        }
        public void Stop()
        {
            if (resourceUpdate != null)
            {
                resourceUpdate.Stop();
                resourceUpdate.Elapsed -= ResourceUpdate_Elapsed;
                resourceUpdate = null;
            }
        }

        private void ResourceUpdate_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (resourceUpdate != null)
            {
                resourceUpdate.Stop();
            }

            Task<string> getResourceTask = TinyDocDB_HttpRequestHelper.PerformResourceRequest("get", rURI, rKey, rPath, rTypeName, String.Empty);
            try
            {
                getResourceTask.Wait();
            }
            catch (ObjectDisposedException ex)
            {
                throw new TinyDocDB_Exception("Resource task had already completed or been destroyed", ex);
            }
            catch (AggregateException ex)
            {
                throw new TinyDocDB_Exception("Errors occured updating the resource", ex);
            }
            if (getResourceTask.Result != cachedResponse)
            {
                cachedResponse = getResourceTask.Result;
                TinyDocDB_UpdateEventArgs updateEventArgs = new TinyDocDB_UpdateEventArgs();
                updateEventArgs.updatedResourceOutput = cachedResponse;
                updateEventArgs.UpdatedTime = DateTime.Now;
                OnResourceUpdate(updateEventArgs);
            }

            if (resourceUpdate != null)
            {
                try
                {
                    resourceUpdate.Start();
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    throw new TinyDocDB_Exception("Invalid polling interval specified (Resource Update Timer)", ex);
                }
            }
        }
    }
}
