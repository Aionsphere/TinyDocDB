# TinyDocDB
TinyDocDB .NET implementation

TinyDocDB is a small simple dependency free .NET client for DocumentDB which uses the REST API's for DocumentDB.
It provides access to DocumentDB in a simplified way and exposes changes to documents via standard .NET events.

## Installation

`Install-Package TinyDocDB`

You can see the full Nuget info here : https://www.nuget.org/packages/TinyDocDB/

## Usage

Please see the example client for use. but basically this is how you use it, its uber simple!

        // Create a reusable connection
        TinyDocDBConnection tinyDbConnection = new TinyDocDB_Connection("YOUR_DOCDB_HOST", "YOUR_MASTER_KEY");

        // Grab a document!
        Console.WriteLine(tinyDbConnection.GetDocument(databaseName, collectionName, documentId));
        
        // Create a document resource so we can monitor a document through standard .NET events!
        TinyDocDB_Resource resource = tinyDbConnection.StartMonitoringDocument(databaseName, collectionName, documentId);
            
        resource.ResourceUpdate += Resource_ResourceUpdate;
        resource.Start();

        ...

        private static void Resource_ResourceUpdate(object sender, TinyDocDB_UpdateEventArgs e)
        {
            Console.WriteLine(e.updatedResourceOutput);
        }