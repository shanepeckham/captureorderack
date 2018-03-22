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
    using MongoDB.Bson;
    using System.Text;
    using Microsoft.ApplicationInsights.DataContracts;
    using System.Collections.Generic;

    public class OrderService
    {

        #region Protected variables
        private string _teamName;
        private IMongoCollection<Order> ordersCollection;
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly TelemetryClient _customTelemetryClient;
        private bool _isCosmosDb;
        private bool _isEventHub;
        #endregion

        #region Constructor
        public OrderService(ILoggerFactory loggerFactory, TelemetryClient telemetryClient)
        {
            // Initialize the class logger and telemetry client
            _logger = loggerFactory.CreateLogger("OrderService");
            _telemetryClient = telemetryClient;
            _telemetryClient.Context.Cloud.RoleName = "captureorder_netcore";

            // Initialize custom telemetry client, if the key is provided
            var customInsightsKey = System.Environment.GetEnvironmentVariable("APPINSIGHTS_KEY");
            if (!string.IsNullOrEmpty(customInsightsKey))
            {
                _customTelemetryClient = new TelemetryClient();
                _customTelemetryClient.InstrumentationKey = customInsightsKey;
                _customTelemetryClient.Context.Cloud.RoleName = "captureorder_netcore";
            }

            // Initialize the class using environment variables
            _teamName = System.Environment.GetEnvironmentVariable("TEAMNAME");

            // Initialize MongoDB
            // Figure out if this is running on CosmosDB
            var mongoURL = MongoClientSingleton.Instance.Settings.Server.ToString();
            _isCosmosDb = mongoURL.Contains("documents.azure.com");

            // Initialize AMQP
            var amqpURL = System.Environment.GetEnvironmentVariable("AMQPURL");
            _isEventHub = amqpURL.Contains("servicebus.windows.net");

            // Log out the env variables
            ValidateVariable(customInsightsKey, "APPINSIGHTS_KEY");
            ValidateVariable(mongoURL, "MONGOURL");
            _logger.LogInformation($"Cosmos DB: {_isCosmosDb}");
            ValidateVariable(amqpURL, "AMQPURL");
            _logger.LogInformation($"Event Hub: {_isEventHub}");
            ValidateVariable(_teamName, "TEAMNAME");
        }
        #endregion

        #region Methods

        // Logs out value of a variable
        public void ValidateVariable(string value, string envName)
        {
            if (string.IsNullOrEmpty(value))
                _logger.LogInformation($"The environment variable {envName} has not been set");
            else
                _logger.LogInformation($"The environment variable {envName} is {value}");
        }
        public async Task<string> AddOrderToMongoDB(Order order)
        {

            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var success = false;
            try
            {
                // Get the MongoDB collection
                Console.WriteLine("Getting orders collection");
                ordersCollection = MongoClientSingleton.Instance.GetDatabase("k8orders").GetCollection<Order>("orders");
                order.Status = "Open";

                if (string.IsNullOrEmpty(order.Source))
                {
                    order.Source = System.Environment.GetEnvironmentVariable("SOURCE");
                }

                var rnd = new Random(DateTime.Now.Millisecond);
                int partition = rnd.Next(11);
                order.Product = $"product-{partition}";

                // Create an order id
                var newOrderId = ObjectId.GenerateNewId();
	            order.OrderId = newOrderId.ToString();

                var db = _isCosmosDb ? "CosmosDB" : "MongoDB";
                Console.WriteLine($"Inserting order into {db} @ {MongoClientSingleton.Instance.Settings.Server.Host}");
                await ordersCollection.InsertOneAsync(order);

                await Task.Run(() =>
                {
                    _telemetryClient.TrackEvent($"CapureOrder: - Team Name {_teamName} - db {db}",new Dictionary<string,string> {
                        {"team", _teamName},
                        {"challenge", "captureorder"},
                        {"type", db}
                     });
                });
                _logger.LogTrace($"CapureOrder {order.OrderId}: - Team Name {_teamName} -  db {db}");
                success = true;
                return order.OrderId;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, ex.Message);
                if(_customTelemetryClient!=null)
                    _customTelemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                if(_customTelemetryClient!=null) {
                    if (_isCosmosDb) {
                        var dependency = new DependencyTelemetry { 
                            Name= "CosmosDB",
                            Type = "MongoDB",
                            Target = MongoClientSingleton.Instance.Settings.Server.ToString(),
                            Data = "Insert order",
                            Duration = timer.Elapsed,
                            Success = success
                        };
                        _customTelemetryClient.TrackDependency(dependency);                        
                    }
                    else {
                        var dependency = new DependencyTelemetry { 
                            Name= "MongoDB",
                            Type = "MongoDB",
                            Target = MongoClientSingleton.Instance.Settings.Server.ToString(),
                            Data = "Insert order",
                            Duration = timer.Elapsed,
                            Success = success
                        };
                        _customTelemetryClient.TrackDependency(dependency);   
                    }
                 }
            }
        }

        public async Task AddOrderToAMQP(Order order)
        {
            // Only execute if AMQP is defined
            if(!string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("AMQPURL"))) {
                if (_isEventHub)
                    await AddOrderToAMQP10(order);
                else
                {
                    await AddOrderToAMQP091(order);
                }
            }
            else {
                _logger.LogTrace($"Skipping AMQP. It is either not configured or improperly configured");                
            }
        }

        private async Task AddOrderToAMQP10(Order order)
        {
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var success = false;
            try
            {
                // Send to AMQP
                var amqpMessage = new Message($"{{\"order\": \"{order.OrderId}\", \"source\": \"{_teamName}\"}}");
                await AMQP10ClientSingleton.Instance.SendAsync(amqpMessage);
                _logger.LogTrace($"Sent message to AMQP 1.0 (EventHub) {AMQP10ClientSingleton.AMQPUrl} {amqpMessage.ToJson()}");
                success = true;

                await Task.Run(() =>
                {
                    _telemetryClient.TrackEvent($"SendOrder: - Team Name {_teamName} - EventHub",new Dictionary<string,string> {
                        {"team", _teamName},
                        {"challenge", "sendorder"},
                        {"type", "eventhub"}
                     });
                });
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, ex.Message, order);
                if(_customTelemetryClient!=null)
                    _customTelemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                if(_customTelemetryClient!=null) {
                        var dependency = new DependencyTelemetry { 
                            Name= "EventHub",
                            Type = "AMQP",
                            Target = AMQP10ClientSingleton.AMQPUrl,
                            Data = "Send message",
                            Duration = timer.Elapsed,
                            Success = success
                        };
                        _customTelemetryClient.TrackDependency(dependency);     
                }
            }
        }

        private async Task AddOrderToAMQP091(Order order)
        {
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var success = false;
            try
            {
                await Task.Run(() =>
                {
                    // Send to AMQP
                    var connection = AMQP091ClientSingleton.AMQPConnectionFactory.CreateConnection();

                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(
                            queue: "order",
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

                        var amqpMessage = $"{{\"order\": \"{order.OrderId}\", \"source\": \"{_teamName}\"}}";
                        var body = Encoding.UTF8.GetBytes(amqpMessage);

                        channel.BasicPublish(
                            exchange: "",
                            mandatory:false,
                            routingKey: "order",
                            basicProperties: null,
                            body: body);

                        _logger.LogTrace($"Sent message to AMQP 0.9.1 (RabbitMQ) {AMQP091ClientSingleton.AMQPUrl} {amqpMessage}");
                    }
                });

                success = true;

                await Task.Run(() =>
                {
                    _telemetryClient.TrackEvent($"SendOrder: - Team Name {_teamName} - RabbitMQ",new Dictionary<string,string> {
                        {"team", _teamName},
                        {"challenge", "sendorder"},
                        {"type", "rabbitmq"}
                     });
                });
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, ex.Message, order);
                if(_customTelemetryClient!=null)
                    _customTelemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                if(_customTelemetryClient!=null) {
                        var dependency = new DependencyTelemetry { 
                            Name= "RabbitMQ",
                            Type = "AMQP",
                            Target = AMQP091ClientSingleton.AMQPUrl,
                            Data = "Send message",
                            Duration = timer.Elapsed,
                            Success = success
                        };
                        _customTelemetryClient.TrackDependency(dependency);     
                }
            }
        }
        #endregion
    }
}