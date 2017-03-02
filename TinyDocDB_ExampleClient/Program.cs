using System;
using System.Configuration;
using System.Text;
using System.Timers;
using TinyDocDB;

namespace TinyDocDB_ExampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string documentDBEndpoint = ConfigurationManager.AppSettings["DocumentDBEndpoint"];
            string documentDBAuthKey = ConfigurationManager.AppSettings["DocumentDBAuthKey"];

            TinyDocDB_Connection tinyDbConnection = new TinyDocDB_Connection(documentDBEndpoint, documentDBAuthKey);

            Console.WriteLine("Getting all databases from endpoint '" + documentDBEndpoint + "'");
            Console.WriteLine("(If you are running this the first time, endpoint and auth key are configured in app.config)");
            Console.WriteLine();
            Console.WriteLine(tinyDbConnection.GetAllDatabases());
            
            tinyDbConnection.CreateDatabase("tempdb");
            Console.WriteLine();
            Console.WriteLine("Created database 'tempdb'");

            Console.WriteLine();
            Console.WriteLine("Get Database 'tempdb'");
            Console.WriteLine();
            Console.WriteLine(tinyDbConnection.GetDatabase("tempdb"));

            Console.WriteLine();
            Console.WriteLine("Get all collections in 'tempdb'");
            Console.WriteLine();
            Console.WriteLine(tinyDbConnection.GetAllCollections("tempdb"));

            tinyDbConnection.CreateCollection("tempdb", "tempcoll");
            Console.WriteLine();
            Console.WriteLine("Created collection 'tempcoll' on database 'tempdb'");

            Console.WriteLine();
            Console.WriteLine("Getting 'tempcoll' collection from 'tempdb'");
            Console.WriteLine();
            Console.WriteLine(tinyDbConnection.GetCollection("tempdb", "tempcoll"));

            Console.WriteLine();
            Console.WriteLine("Creating document 'WakefieldFamily' in collection 'tempcoll'");
            Console.WriteLine();
            string jsonWakefieldFamily = System.IO.File.ReadAllText(@"./wakefieldfamily.json");
            Console.WriteLine(tinyDbConnection.CreateDocument("tempdb", "tempcoll", jsonWakefieldFamily));

            Console.WriteLine();
            Console.WriteLine("Getting document 'WakefieldFamily' from collection 'tempcoll'");
            Console.WriteLine();
            Console.WriteLine(tinyDbConnection.GetDocument("tempdb", "tempcoll", "WakefieldFamily"));

            Console.WriteLine();
            Console.WriteLine("Updating document 'WakefieldFamily' in collection 'tempcoll' from updated local document");
            Console.WriteLine();
            string updated_jsonWakefieldFamily = System.IO.File.ReadAllText(@"./updated_wakefieldfamily.json");
            Console.WriteLine(tinyDbConnection.UpdateDocument("tempdb", "tempcoll", "WakefieldFamily", updated_jsonWakefieldFamily));

            Console.WriteLine();
            Console.WriteLine("Creating document 'AndersonFamily' in collection 'tempcoll'");
            Console.WriteLine();
            string jsonAndersonFamily = System.IO.File.ReadAllText(@"./andersonfamily.json");
            Console.WriteLine(tinyDbConnection.CreateDocument("tempdb", "tempcoll", jsonAndersonFamily));

            Console.WriteLine();
            Console.WriteLine("Get all documents in 'tempcoll'");
            Console.WriteLine();
            Console.WriteLine(tinyDbConnection.GetAllDocuments("tempdb", "tempcoll"));

            Console.WriteLine();
            Console.WriteLine("executing json select query on collection 'tempcoll'");
            Console.WriteLine();
            //   This is the json query we read in from jsonselect.json - here for quick reference
            //
            //          {
            //                "query": "SELECT * FROM Families f WHERE f.id = @familyId",     
            //                "parameters": [
            //                    {"name": "@familyId", "value": "AndersenFamily"}         
            //                ] 
            //           }
            string jsonQuery = System.IO.File.ReadAllText(@"./jsonselect.json");
            Console.WriteLine(tinyDbConnection.QueryCollection("tempdb", "tempcoll", jsonQuery));


            Console.WriteLine();
            Console.WriteLine("executing json select with join on collection 'tempcoll'");
            Console.WriteLine();
            //   This is the json query we read in from jsonselectwithjoin.json - here for quick reference
            //          {
            //                "query": "SELECT 
            //                             f.id AS familyName, 
            //                 c.givenName AS childGivenName, 
            //                 c.firstName AS childFirstName, 
            //                 p.givenName AS petName
            //              FROM Families f
            //                          JOIN c IN f.children
            //              JOIN p in c.pets",     
            //                "parameters": []
            //           }
            string jsonQuery2 = System.IO.File.ReadAllText(@"./jsonselectwithjoin.json");
            Console.WriteLine(tinyDbConnection.QueryCollection("tempdb", "tempcoll", jsonQuery2));

            Console.WriteLine("In DocumentDB change something (dont break the json ;) in the AndersonFamily document tempdb / tempcoll / AndersonFamily");
            Console.WriteLine("You can change it from the Document DB Instances Document Browser in the Azure Portal.");
            Console.WriteLine("You should receive an event which will display the updated document!");
            Console.WriteLine("Press ESC to stop (at which time I'll delete everything I created!");
            TinyDocDB_Resource tinyResource = tinyDbConnection.StartMonitoringDocument("tempdb", "tempcoll", "AndersenFamily");
            tinyResource.ResourceUpdate += TinyResource_ResourceUpdate;
            tinyResource.Start();

            do
            {
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            tinyDbConnection.DeleteDocument("tempdb", "tempcoll", "WakefieldFamily");
            Console.WriteLine();
            Console.WriteLine("Deleted document 'WakefieldFamily' on collection 'tempcoll'");

            tinyDbConnection.DeleteCollection("tempdb", "tempcoll");
            Console.WriteLine();
            Console.WriteLine("Deleted collection 'tempcoll' on database 'tempdb'");

            tinyDbConnection.DeleteDatabase("tempdb");
            Console.WriteLine();
            Console.WriteLine("Deleted database 'tempdb' from endpoint '" + documentDBEndpoint + "'");
        }

        private static void TinyResource_ResourceUpdate(object sender, TinyDocDB_UpdateEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("***************************************************");
            Console.WriteLine("Document Update Received " + e.updatedResourceOutput);
            Console.WriteLine("***************************************************");
            Console.WriteLine();
            Console.WriteLine("Press ESC to stop (at which time I'll delete everything I created!");
            Console.WriteLine();
        }
    }
}
