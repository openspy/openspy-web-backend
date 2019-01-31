using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Database
{
    public class SessionCacheDatabase : IRedisDBContext
    {
        private const int REDIS_GAME_DB = 3;
        private ConnectionMultiplexer redisMultiplexer;
        public SessionCacheDatabase(ConnectionMultiplexer redisMultiplexer)
        {
            this.redisMultiplexer = redisMultiplexer;
        }
        public IDatabase GetDatabase()
        {
            return redisMultiplexer.GetDatabase(REDIS_GAME_DB);
        }
    }
}