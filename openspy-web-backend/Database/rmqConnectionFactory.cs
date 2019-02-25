using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Database
{
    public class rmqConnectionFactory : IMQConnectionFactory
    {

        private IConfiguration configuration;

        public rmqConnectionFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public ConnectionFactory Get()
        {
            UriBuilder uriBuilder = new UriBuilder(configuration.GetConnectionString("rmqConnection"));
            return new ConnectionFactory
            {
                Uri = uriBuilder.Uri
            };
        }
    }
}
