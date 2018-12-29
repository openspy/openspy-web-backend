using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Database
{
    public interface IMQConnectionFactory
    {
        ConnectionFactory Get();
    }
}
