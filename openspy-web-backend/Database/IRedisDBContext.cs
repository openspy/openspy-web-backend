using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Database
{
    public interface IRedisDBContext
    {
        IDatabase GetDatabase();
    }
}
