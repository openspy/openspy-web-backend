using CoreWeb.Database;
using CoreWeb.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using RabbitMQ.Client;
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
        public List<SnapshotUpdate> updates;
        public int profileid;
        public int gameid;
        public String ip;
        public bool processed;
    };
    public class SnapshotLookup
    {
        public GameLookup gameLookup;
        public ProfileLookup profileLookup;
        public string _id;
        public bool? processed;
    };
    public class SnapShotRepository : IRepository<Snapshot, SnapshotLookup>
    {
        private String GSTATS_EXCHANGE;
        private String GSTATS_ROUTING_KEY;
        private IMongoCollection<BsonDocument> collection;
        private IMQConnectionFactory connectionFactory;
        private IRepository<Game, GameLookup> gameRepository;
        public SnapShotRepository(ISnapShotDBContext snapshotDb, IMQConnectionFactory connectionFactory, IRepository<Game, GameLookup> gameRepository)
        {
            GSTATS_EXCHANGE = "openspy.gamestats";
            GSTATS_ROUTING_KEY = "snapshots";
            var db = snapshotDb.GetDatabase();
            collection = db.GetCollection<BsonDocument>("snapshots");
            this.connectionFactory = connectionFactory;
            this.gameRepository = gameRepository;
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

        public async Task<IEnumerable<Snapshot>> Lookup(SnapshotLookup lookup)
        {
            var searchRequest = new BsonDocument
            {
            };

            if(lookup._id != null)
            {
                searchRequest["_id"] = new BsonObjectId(lookup._id);
            } else {
                Game game = (await gameRepository.Lookup(lookup.gameLookup)).FirstOrDefault();
                if(game != null) {
                    searchRequest["gameid"] = new BsonInt32(game.Id);
                }
            }

            if(lookup.processed.HasValue) {
                searchRequest["processed"] = new BsonBoolean(lookup.processed.Value);
            }

            var results = (await collection.FindAsync(searchRequest)).ToList();
            var output = new List<Snapshot>();
            foreach(var result in results)
            {
                var snapshot = new Snapshot();
                snapshot._id = result["_id"].AsObjectId;
                snapshot.gameid = result["gameid"].AsInt32;
                if(result.Contains("processed")) {
                    snapshot.processed = result["processed"].AsBoolean;
                } else {
                    snapshot.processed = false;
                }
                if(result.Contains("profileid")) {
                    snapshot.profileid = result["profileid"].AsInt32;
                }
                
                snapshot.ip = result["ip"].AsString;
                if(result["created"].IsDateTime)
                    snapshot.created = result["created"].AsDateTime;
                snapshot.updates = new List<SnapshotUpdate>();
                var updates = result["updates"].AsBsonArray;

                foreach(var update in updates) {
                    var sub_update = new SnapshotUpdate();
                    if(update["created"].IsDateTime)
                        sub_update.created = update["created"].AsDateTime;
                    if(update.AsBsonDocument.Contains("profileid")) {
                        sub_update.profileid = update["profileid"].AsInt32;
                    } else {
                        sub_update.profileid = snapshot.profileid;
                    }
                    if(update.AsBsonDocument.Contains("gameid")) {
                        sub_update.gameid = update["gameid"].AsInt32;
                    } else {
                        sub_update.gameid = snapshot.gameid;
                    }
                    if(update.AsBsonDocument.Contains("complete")) {
                        sub_update.completed = update["complete"].AsBoolean;
                    }
                    
                    sub_update.data = new Dictionary<string, string>();
                    var data = update["data"].AsBsonDocument;
                    var elements = data.Elements;
                    foreach(var element in elements) {
                        sub_update.data[element.Name] = element.Value.AsString;
                    }
                    snapshot.updates.Add(sub_update);
                }
                output.Add(snapshot);
            }
            return output;
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

            var success = result.IsAcknowledged && result.IsModifiedCountAvailable && result.ModifiedCount > 0;
            if(success && snapshotUpdate.completed) {
                ConnectionFactory factory = connectionFactory.Get();
                using (IConnection connection = factory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        var lookup = new SnapshotLookup();
                        lookup._id = _id;
                        var fullSnapshot = (await Lookup(lookup)).First();
                        var jsonString =   Newtonsoft.Json.JsonConvert.SerializeObject(fullSnapshot);
                        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

                        IBasicProperties props = channel.CreateBasicProperties();
                        props.ContentType = "application/json";
                        props.Headers = new Dictionary<string, object>();
                        props.Headers["X-OpenSpy-GameId"] = snapshotUpdate.gameid;
                        channel.BasicPublish(GSTATS_EXCHANGE, GSTATS_ROUTING_KEY, props, messageBodyBytes);
                        return true;
                    }
                }
            }
            return success;
        }
        public async Task RequeueSnapshots(SnapshotLookup request) {
            var snapshots = await Lookup(request);
            Game game = (await gameRepository.Lookup(request.gameLookup)).FirstOrDefault();

            ConnectionFactory factory = connectionFactory.Get();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    var fullSnapshots = (await Lookup(request));
                    foreach(var snapshot in fullSnapshots) {
                    var jsonString =   Newtonsoft.Json.JsonConvert.SerializeObject(snapshot);
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "application/json";
                    props.Headers = new Dictionary<string, object>();
                    props.Headers["X-OpenSpy-GameId"] = game.Id;
                    channel.BasicPublish(GSTATS_EXCHANGE, GSTATS_ROUTING_KEY, props, messageBodyBytes);
                    }
                }
            }
        }
    }
}
