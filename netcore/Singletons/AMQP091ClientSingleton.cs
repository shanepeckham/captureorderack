using System;
using System.Security.Authentication;
using RabbitMQ.Client;

namespace OrderCaptureAPI.Singetons
{
    public sealed class AMQP091ClientSingleton : IDisposable
    {
        private static readonly AMQP091ClientSingleton instance = new AMQP091ClientSingleton();
        private static volatile ConnectionFactory _factory;
        private static volatile string _amqpUrl;

        public static string AMQPUrl
        {
            get
            {
                return _amqpUrl;
            }
        }

        public static ConnectionFactory AMQPConnectionFactory
        {
            get
            {
                return _factory;
            }
        }

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static AMQP091ClientSingleton()
        {
        }

        private AMQP091ClientSingleton()
        {
            // Retrieve the AMQPHost from the Environment Variables
            _amqpUrl = System.Environment.GetEnvironmentVariable("AMQPURL");
            _factory = new ConnectionFactory();

            // Validate and throw an exception if invalid
            if (!System.Uri.IsWellFormedUriString(_amqpUrl, UriKind.Absolute))
                throw new ArgumentException("Unable to parse AMQPURL as a Uri.");

            var uri = new Uri(_amqpUrl);
            var user = "";
            var pass = "";
            if(uri.UserInfo != null && !string.IsNullOrWhiteSpace(uri.UserInfo)) {
                var userpass = uri.UserInfo.Split(':');
                if(userpass.Length==2) {
                    user = userpass[0];
                    pass = userpass[1];
                }
            }

            _factory.UserName = user;
            _factory.Password = pass;
            _factory.HostName = uri.Host;
            _factory.RequestedConnectionTimeout = 3000;
        }
        public void Dispose()
        {
            
        }
    }
}