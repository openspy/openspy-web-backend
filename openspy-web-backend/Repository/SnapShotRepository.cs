using CoreWeb.Database;
using CoreWeb.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Repository
{
    public class SnapshotUpdate
    {
        public SnapshotUpdate()
        {
            data = new Dictionary<string, string>();
        }
        public DateTime created;
        public int profileid;
        public int gameid;
        public Dictionary<string, string> data;
        public bool completed;
    };
    public class Snapshot
    {
        public ObjectId _id;
        public DateTime created;
        public SnapshotUpdate updates;
        public int profileid;
        public int gameid;
        public String ip;
        public bool processed;
    };
    public class SnapshotLookup
    {
        public GameLookup gameLookup;
        public ProfileLookup profileLookup;
        public ObjectId _id;
        public bool? processed;
    };
    public class SnapShotRepository : IRepository<Snapshot, SnapshotLookup>
    {
        private IMongoCollection<BsonDocument> collection;
        public SnapShotRepository(ISnapShotDBContext snapshotDb)
        {
            var db = snapshotDb.GetDatabase();
            collection = db.GetCollection<BsonDocument>("snapshots");
        }
        public async Task<Snapshot> Create(Snapshot model)
        {
            var document = new BsonDocument
            {
                { "gameid", model.gameid },
                { "profileid", model.profileid },
                {"ip", model.ip },
                {"created", new BsonDateTime(DateTime.UtcNow)  },
                { "updates", new BsonArray() }
            };
            await collection.InsertOneAsync(document);
            model._id = document["_id"].AsObjectId;
            return model;
        }

        public Task<bool> Delete(SnapshotLookup lookup)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Snapshot>> Lookup(SnapshotLookup lookup)
        {
            throw new NotImplementedException();
        }

        public Task<Snapshot> Update(Snapshot model)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> AppendSnapshotUpdate(string _id, SnapshotUpdate snapshotUpdate)
        {
            var data = new BsonDocument();
            foreach(var kvEntry in snapshotUpdate.data)
            {
                var element = new BsonElement(kvEntry.Key, new BsonString(kvEntry.Value));
                data.Add(element);
            }
            var entry = new BsonDocument
            {
                {"created", new BsonDateTime(DateTime.UtcNow)  },
                { "data",  data},
                {"completed", new BsonBoolean(snapshotUpdate.completed) }
            };
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(_id));
            var update = Builders<BsonDocument>.Update.Push("updates", entry);
            var result = await collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.IsModifiedCountAvailable && result.ModifiedCount > 0;
        }
    }
}
