using System;
using System.Timers;
using TinyDocDB;

namespace TinyDocDB_ExampleClient
{
    class Program
    {
        /// <summary>
        /// The below json should be added to DocumentDB under an existing database and collection
        /// If you are creating from scratch, create a new DocumentDB instance and insert your database name
        /// in the databaseName variable below, then create a new collection under that database and insert it
        /// in the collectionName variable below, then create a new document under document explorer 
        /// and paste the json below into it and save it.
        /// 
        ///    {
        ///       "id": "demojson",
        ///       "valueToChange": "1"
        ///    }
        ///    
        /// Finally go to your database, and select 'Keys' and copy the URI and primary key values and put
        /// them in the place holder variables below.
        /// </summary>

        private const string serviceURI = "https://YOURHOSTHERE.documents.azure.com";
        private const string primaryKey = "YOUR PRIMARY KEY HERE";
        private const string databaseName = "tempdb";
        private const string collectionName = "tempcoll";
        private const string documentId = "AndersenFamily";

        private static Timer updateTimer;
        private static bool flipFlop = false;
        private static string jsonConfig = String.Empty;
        private static string jsonConfig2 = String.Empty;
        private static bool gotConfiguration = false;
        private static TinyDocDB_Connection tinyDbConnection = null;
        static void Main(string[] args)
        {
            tinyDbConnection = new TinyDocDB_Connection(serviceURI, primaryKey);
            TinyDocDB_Resource resource = tinyDbConnection.StartMonitoringDocument(databaseName, collectionName, documentId);
            Console.WriteLine(tinyDbConnection.GetDocument(databaseName, collectionName, documentId));
            Console.WriteLine();
            
            resource.ResourceUpdate += Resource_ResourceUpdate;
            resource.Start();

            Program.updateTimer = new Timer(3000);
            updateTimer.Elapsed += UpdateTimer_Elapsed;

            Console.WriteLine("Press ESC to stop");
            do
            {
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            if (updateTimer != null)
            {
                updateTimer.Stop();
                updateTimer = null;
            }
            
            if (resource != null)
            {
                resource.Stop();
                resource.ResourceUpdate -= Resource_ResourceUpdate;
                resource = null;
            }

            if (tinyDbConnection != null)
            { 
                tinyDbConnection = null;
            }
        }

        private static void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            updateTimer.Stop();
            if (flipFlop)
            {
                string result = tinyDbConnection.UpdateDocument(databaseName, collectionName, documentId, jsonConfig);
            }
            else
            {
                string result = tinyDbConnection.UpdateDocument(databaseName, collectionName, documentId, jsonConfig2);
            }

            flipFlop = !flipFlop;
            updateTimer.Start();
        }

        private static void Resource_ResourceUpdate(object sender, TinyDocDB_UpdateEventArgs e)
        {
            Console.WriteLine("Configuration Change at {0}", e.UpdatedTime);
            Console.WriteLine("New Configuration follows..");
            Console.WriteLine("...");
            string tempProdId = e.updatedResourceOutput.ToString();
            int i = tempProdId.IndexOf("chain");
            tempProdId = tempProdId.Substring(1, i);
            Console.WriteLine(tempProdId);
            if (!gotConfiguration)
            {
                jsonConfig = e.updatedResourceOutput;
                int j = e.updatedResourceOutput.IndexOf("56");
                if (j < 1)
                {
                    jsonConfig2 = jsonConfig.Replace("55", "56");
                }
                else
                {
                    jsonConfig2 = jsonConfig.Replace("56", "55");
                }

                gotConfiguration = true;
                updateTimer.Start();
            }
        }
    }
}
