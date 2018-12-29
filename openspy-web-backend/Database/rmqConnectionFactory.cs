using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Database
{
    public class rmqConnectionFactory : IMQConnectionFactory
    {
        public ConnectionFactory Get()
        {
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.UserName = "guest";
            uriBuilder.Password = "guest";
            uriBuilder.Host = "localhost";
            uriBuilder.Port = 5672;
            uriBuilder.Path = "/";
            uriBuilder.Scheme = "amqp";
            return new ConnectionFactory
            {
                Uri = uriBuilder.Uri
            };
        }
    }
}
