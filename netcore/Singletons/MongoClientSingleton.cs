using System;
using System.Security.Authentication;
using MongoDB.Bson;
using MongoDB.Driver;
using OrderCaptureAPI.Models;

namespace OrderCaptureAPI.Singetons
{
    public sealed class MongoClientSingleton
    {
        private static readonly MongoClientSingleton instance = new MongoClientSingleton();

        private static volatile MongoClient _mongoClientInstance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static MongoClientSingleton()
        {
        }

        private MongoClientSingleton()
        {
            // Retrieve the MongoURL from the Environment Variables
            var mongoURL = System.Environment.GetEnvironmentVariable("MONGOURL");

            // Validate and throw an exception if invalid
            if (!System.Uri.IsWellFormedUriString(mongoURL, UriKind.Absolute))
                throw new ArgumentException("Unable to parse MONGOURL as a Uri.");

            // Initialize the MongoClient singleton
            _mongoClientInstance =  new MongoClient(mongoURL);

            // Create the database 
            var db = MongoClientSingleton.Instance.GetDatabase("k8orders");

            // Create a sharded collection
            try {
                Console.WriteLine("Trying to create a sharded collection.");
                var shardedCollectionCommand = new JsonCommand<BsonDocument>(@"{ shardCollection: ""k8orders.orders"", key: { product: ""hashed"" } }");
                var result = db.RunCommand(shardedCollectionCommand);
                Console.WriteLine(result.ToString());
            }
            catch(MongoCommandException ex) {
                // The collection is most likely already sharded. I couldn't find a more elegant way to check this.                
		        Console.WriteLine("Could not create/re-create sharded MongoDB collection. Either collection is already sharded or sharding is not supported. You can ignore this error. ", ex.Message);
            }
        }

        public static MongoClient Instance
        {
            get
            {
                return _mongoClientInstance;
            }
        }
    }
}