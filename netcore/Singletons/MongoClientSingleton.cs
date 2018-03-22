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
            //_mongoClientInstance =  new MongoClient(mongoURL);


            // Parse to get components
            var parsedMongoURL = new Uri(mongoURL);
            Console.WriteLine($"MONGO HOST: {parsedMongoURL.Host}");

            // Create settings object to connect, passing 5 seconds for server connection timeout
            var settings = new MongoClientSettings {
                Server = new MongoServerAddress(parsedMongoURL.Host, parsedMongoURL.Port),
                ClusterConfigurator = builder =>
                {
                    builder.ConfigureCluster(s => s.With(serverSelectionTimeout: TimeSpan.FromSeconds(10)));
                }
            };

            // Use SSL if required
            if(parsedMongoURL.PathAndQuery.Contains("ssl=true")) {
                settings.UseSsl = true;
                settings.SslSettings = new SslSettings {
                    EnabledSslProtocols = SslProtocols.Tls12
                };
            }

            // Pass credentials if specified    
            if(!string.IsNullOrWhiteSpace(parsedMongoURL.UserInfo)) {
                var userPass = parsedMongoURL.UserInfo.Split(":");
                Console.WriteLine($"MONGO USER: {userPass[0]}");
                Console.WriteLine($"MONGO PASS: {userPass[1]}");
                var identity = new MongoInternalIdentity("k8orders", userPass[0]);
                var evidence = new PasswordEvidence(userPass[1]);
                settings.Credential =  new MongoCredential("SCRAM-SHA-1", identity, evidence);
            }

            // Initialize with settings
            _mongoClientInstance = new MongoClient(settings);

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
            catch(Exception ex) {
		        Console.WriteLine("Could not connect to MongoDB: ", ex.Message);                
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