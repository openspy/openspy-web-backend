using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CoreWeb.Database
{
    public class GameCacheDatabase : IRedisDBContext
    {
        private const int REDIS_GAME_DB = 2;
        private ConnectionMultiplexer redisMultiplexer;
        public GameCacheDatabase(ConnectionMultiplexer redisMultiplexer)
        {
            this.redisMultiplexer = redisMultiplexer;
        }
        public IDatabase GetDatabase()
        {
            return redisMultiplexer.GetDatabase(REDIS_GAME_DB);
        }
        public void FlushDatabase()
        {
            var endpoint = redisMultiplexer.GetEndPoints().FirstOrDefault();
            if(endpoint != null)
            {
                var server = redisMultiplexer.GetServer(endpoint);
                if(server != null)
                {
                    server.FlushDatabase(REDIS_GAME_DB);
                }
            }
        }
    }
}
