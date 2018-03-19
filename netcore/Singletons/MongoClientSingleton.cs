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
            var mongoURL = System.Environment.GetEnvironmentVariable("MONGOHOST");

            // Validate and throw an exception if invalid
            if (!System.Uri.IsWellFormedUriString(mongoURL, UriKind.Absolute))
                throw new ArgumentException("Unable to parse MONGOHOST as a Uri.");

            // Initialize the MongoClient singleton
            _mongoClientInstance =  new MongoClient(mongoURL);

            // Create the database 
            var db = MongoClientSingleton.Instance.GetDatabase("k8orders");

            // Create a sharded collection
            try {
                var shardedCollectionCommand = new JsonCommand<BsonDocument>(@"{ shardCollection: ""k8orders.orders"", key: { product: ""hashed"" } }");
                db.RunCommand(shardedCollectionCommand);
            }
            catch(MongoCommandException) {
                // The collection is most likely already sharded. I couldn't find a more elegant way to check this.
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