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

    public class OrderService
    {

        #region Protected variables
        private string _teamName;
        private string _amqpHost;
        private IMongoCollection<Order> ordersCollection;
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetryClient;
        private bool _isCosmosDb;
        private bool _isEventHub;
        #endregion

        #region Constructor
        public OrderService(ILogger<Controllers.OrderController> logger, TelemetryClient telemetryClient)
        {
            // Initialize the class logger and telemetry client
            _logger = logger;
            _telemetryClient = telemetryClient;

            // Initialize the class using environment variables
            _teamName = System.Environment.GetEnvironmentVariable("TEAMNAME");
            _amqpHost = System.Environment.GetEnvironmentVariable("RABBITMQHOST");
        }
        #endregion

        #region Methods
        public async Task<string> AddOrderToMongoDB(Order order)
        {
            try
            {
                // Figure out if this is running on CosmosDB
                _isCosmosDb = MongoClientSingleton.Instance.Settings.Server.ToString().Contains("documents.azure.com");

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
                    _telemetryClient.TrackEvent("CapureOrder: - Team Name " + _teamName + " db " + db);
                });
                _logger.LogTrace($"Added order to {db}");

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
                 // Figure out if this is running on EventHub
                _isEventHub = _amqpHost.Contains("servicebus.windows.net");

                // If running on Azure, get a random partition from 0 to 2 and append to address
                if(_isEventHub) {
                    var rnd = new Random(DateTime.Now.Millisecond);
                    int partition = rnd.Next(0, 2);
                    _amqpHost += _amqpHost + "/Partitions/" + partition.ToString();
                }

                var amqpConnection = await Connection.Factory.CreateAsync(new Address(_amqpHost));
                var amqpSession = new Session(amqpConnection);
                var amqpMessage = new Message($"{{'order': '{order.Id}', 'source': '{_teamName}'}}");
                var amqpSender = new SenderLink(amqpSession, "sender-link", "q1");

                // Send to AMQP (fire and forget)
                amqpSender.Send(amqpMessage, null, null);
                _logger.LogTrace("Sent message to AMQP");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.InnerException, ex.InnerException.Message, order);
                throw new Exception(ex.InnerException.Message, ex.InnerException);
            }
        }
        #endregion
    }
}