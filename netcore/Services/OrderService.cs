namespace OrderCaptureAPI.Services
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
    using OrderCaptureAPI.Models;
    using OrderCaptureAPI.Singetons;
    using Microsoft.ApplicationInsights;
    using Amqp;
    using Amqp.Framing;

    public class OrderService
    {

        #region Protected variables
        private string _teamName;
        private IMongoCollection<Order> ordersCollection;
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetryClient;
        private bool _isCosmosDb;
        private bool _isEventHub;
        #endregion

        #region Constructor
        public OrderService(ILoggerFactory loggerFactory, TelemetryClient telemetryClient)
        {
            // Initialize the class logger and telemetry client
            _logger = loggerFactory.CreateLogger("OrderService");
            _telemetryClient = telemetryClient;

            // Initialize the class using environment variables
            _teamName = System.Environment.GetEnvironmentVariable("TEAMNAME");

            // Initialize MongoDB
            // Figure out if this is running on CosmosDB
            var mongoHost = MongoClientSingleton.Instance.Settings.Server.ToString();
            _isCosmosDb = mongoHost.Contains("documents.azure.com");
            _logger.LogInformation($"Cosmos DB: {_isCosmosDb}");
            _logger.LogInformation($"MongoHost: {mongoHost}");

            // Initialize AMQP
            var amqpHost = AMQPClientSingleton.AMQPHost;        
            _isEventHub = mongoHost.Contains("servicebus.windows.net");
            _logger.LogInformation($"Event Hub: {_isEventHub}");
            _logger.LogInformation($"AMQP Host: {AMQPClientSingleton.AMQPHost}");
        }
        #endregion

        #region Methods
        public async Task<string> AddOrderToMongoDB(Order order)
        {
            try
            {
                // Get the MongoDB collection
                ordersCollection = MongoClientSingleton.Instance.GetDatabase("k8orders").GetCollection<Order>("orders");
                order.Status = "Open";

                if (string.IsNullOrEmpty(order.Source))
                {
                    order.Source = System.Environment.GetEnvironmentVariable("SOURCE");
                }

                await ordersCollection.InsertOneAsync(order);
                var db = _isCosmosDb ? "CosmosDB" : "MongoDB";
                await Task.Run(() =>
                {
                    _telemetryClient.TrackEvent($"CapureOrder: - Team Name {_teamName} -  db {db}");
                });
                _logger.LogTrace($"CapureOrder {order.Id}: - Team Name {_teamName} -  db {db}");

                return order.Id;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.InnerException, ex.InnerException.Message, order);
                throw new Exception(ex.InnerException.Message, ex.InnerException);
            }
        }

        public async Task AddOrderToAMQP(Order order)
        {
            try
            {
                // Send to AMQP
                var amqpMessage = new Message($"{{'order': '{order.Id}', 'source': '{_teamName}'}}");
                await AMQPClientSingleton.Instance.SendAsync(amqpMessage);
                _logger.LogTrace("Sent message to AMQP");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, ex.Message, order);
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion
    }
}