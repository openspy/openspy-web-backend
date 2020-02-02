using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Database
{
    public class PeerchatCacheDatabase : IRedisDBContext
    {
        private int PEERCHAT_CACHE_REDISDB = 4;
        private ConnectionMultiplexer redisMultiplexer;
        public PeerchatCacheDatabase(ConnectionMultiplexer redisMultiplexer)
        {
            this.redisMultiplexer = redisMultiplexer;
        }

        public IDatabase GetDatabase()
        {
            return redisMultiplexer.GetDatabase(PEERCHAT_CACHE_REDISDB);
        }
    }
}
