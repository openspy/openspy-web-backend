using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Database
{
    public interface ISnapShotDBContext
    {
        MongoClient GetMongoClient();
        IMongoDatabase GetDatabase();
    }
}
