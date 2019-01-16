using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Database
{
    public class SnapShotDBContext : ISnapShotDBContext
    {
        private string uri;
        private MongoClient client;
        public SnapShotDBContext(string uri)
        {
            this.uri = uri;
            client = new MongoClient(uri);
        }
        public MongoClient GetMongoClient()
        {
            return client;
        }
        public IMongoDatabase GetDatabase()
        {
            return client.GetDatabase("gamestats");
        }
    }
}
