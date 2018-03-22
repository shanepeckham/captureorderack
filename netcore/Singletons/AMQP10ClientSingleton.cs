using System;
using System.Security.Authentication;
using Amqp;

namespace OrderCaptureAPI.Singetons
{
    public sealed class AMQP10ClientSingleton : IDisposable
    {
        private static readonly AMQP10ClientSingleton instance = new AMQP10ClientSingleton();
        private static volatile SenderLink _senderLinkInstance;
        private static volatile Address _amqpAddress;
        private static volatile Connection _amqpConnection;
        private static volatile Session _amqpSession;
        private static volatile string _amqpUrl;

        public static string AMQPUrl
        {
            get
            {
                return _amqpUrl;
            }
        }

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static AMQP10ClientSingleton()
        {
        }

        private AMQP10ClientSingleton()
        {
            // Retrieve the AMQPURL from the Environment Variables
            _amqpUrl = System.Environment.GetEnvironmentVariable("AMQPURL");

            // Validate and throw an exception if invalid
            if (!System.Uri.IsWellFormedUriString(_amqpUrl, UriKind.Absolute))
                throw new ArgumentException("Unable to parse AMQPURL as a Uri. Make sure your policy key is URL Encoded.");

            var uri = new Uri(_amqpUrl);
            var _eventHubEntity = uri.PathAndQuery;

            var rnd = new Random(DateTime.Now.Millisecond);
            int partition = rnd.Next(3);
            _amqpUrl += "/partitions/" + partition.ToString();

            // Initialize the SenderLink singleton
            _amqpAddress = new Address(_amqpUrl);
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

        public static Amqp.Framing.Error LastSenderLinkError
        {
            get
            {
                return _senderLinkInstance.Error;
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