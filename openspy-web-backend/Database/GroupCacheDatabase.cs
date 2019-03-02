using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Database
{
    public class GroupCacheDatabase : IRedisDBContext
    {
        private const int REDIS_GROUP_DB = 1;
        private ConnectionMultiplexer redisMultiplexer;
        public GroupCacheDatabase(ConnectionMultiplexer redisMultiplexer)
        {
            this.redisMultiplexer = redisMultiplexer;
        }
        public IDatabase GetDatabase()
        {
            return redisMultiplexer.GetDatabase(REDIS_GROUP_DB);
        }
        public void FlushDatabase()
        {
            var endpoint = redisMultiplexer.GetEndPoints().FirstOrDefault();
            if (endpoint != null)
            {
                var server = redisMultiplexer.GetServer(endpoint);
                if (server != null)
                {
                    server.FlushDatabase(REDIS_GROUP_DB);
                }
            }
        }
    }
}
