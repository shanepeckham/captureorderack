using System;
using System.Security.Authentication;
using Amqp;

namespace OrderCaptureAPI.Singetons
{
    public sealed class AMQPClientSingleton : IDisposable
    {
        private static readonly AMQPClientSingleton instance = new AMQPClientSingleton();
        private static volatile SenderLink _senderLinkInstance;
        private static volatile Address _amqpAddress;
        private static volatile Connection _amqpConnection;
        private static volatile Session _amqpSession;
        private static volatile string _amqpHost;
        private static volatile string _eventHubEntity;

        public static string AMQPHost
        {
            get
            {
                return _amqpHost;
            }
        }

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static AMQPClientSingleton()
        {
        }

        private AMQPClientSingleton()
        {
            // Retrieve the AMQPHost from the Environment Variables
            _amqpHost = System.Environment.GetEnvironmentVariable("AMQPHOST");
            _eventHubEntity = System.Environment.GetEnvironmentVariable("EVENTHUBNAME");
            

            // Figure out if this is running on EventHub
            var isEventHub = _amqpHost.Contains("servicebus.windows.net");

            // If running on Azure, get a random partition from 0 to 2 and append to address
            if (isEventHub)
            {
                var rnd = new Random(DateTime.Now.Millisecond);
                int partition = rnd.Next(0, 0);
                _amqpHost += _amqpHost + "/Partitions/" + partition.ToString();
            }

            // Validate and throw an exception if invalid
            if (!System.Uri.IsWellFormedUriString(_amqpHost, UriKind.Absolute))
                throw new ArgumentException("Unable to parse AMQPHOST as a Uri.");

            // Initialize the SenderLink singleton
            _amqpAddress = new Address(_amqpHost);
            _amqpConnection = new Connection(_amqpAddress);
            _amqpSession = new Session(_amqpConnection);
            _senderLinkInstance = new SenderLink(_amqpSession, "order", _eventHubEntity);
        }

        public static SenderLink Instance
        {
            get
            {
                return _senderLinkInstance;
            }
        }

        public void Dispose()
        {
            _senderLinkInstance.Close();
            _amqpSession.Close();
            _amqpConnection.Close();
        }
    }
}