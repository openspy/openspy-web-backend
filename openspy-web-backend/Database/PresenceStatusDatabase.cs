using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Database
{
    public class PresenceStatusDatabase : IRedisDBContext
    {
        private int PRESENCE_STATUS_REDISDB = 5;
        private ConnectionMultiplexer redisMultiplexer;
        public PresenceStatusDatabase(ConnectionMultiplexer redisMultiplexer)
        {
            this.redisMultiplexer = redisMultiplexer;
        }

        public IDatabase GetDatabase()
        {
            return redisMultiplexer.GetDatabase(PRESENCE_STATUS_REDISDB);
        }
    }
}
