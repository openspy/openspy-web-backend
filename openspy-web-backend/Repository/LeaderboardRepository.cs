using CoreWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace CoreWeb.Repository
{
    public class Leaderboard
    {
        public object data;
        public Game game;
        public int total;
    };
    public class LeaderboardLookup
    {
        public string objectId;
        public GameLookup gameLookup;
        public string baseKey;
        public Dictionary<string, object> data; //data matches
        public int? pageOffset;
        public int? pageSize;
    };
    public class LeaderboardRepository : IRepository<Leaderboard, LeaderboardLookup>
    {
        private IRepository<Profile, ProfileLookup> profileRepository;
        private IRepository<Game, GameLookup> gameRepository;
        private IMongoCollection<BsonDocument> collection;
        public LeaderboardRepository(IRepository<Profile, ProfileLookup> profileRepository, IRepository<Game, GameLookup> gameRepository, ISnapShotDBContext snapshotDb)
        {
            this.profileRepository = profileRepository;
            this.gameRepository = gameRepository;

            var db = snapshotDb.GetDatabase();
            collection = db.GetCollection<BsonDocument>("leaderboards");
        }
        public Task<Leaderboard> Create(Leaderboard model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(LeaderboardLookup lookup)
        {
            throw new NotImplementedException();
        }

        private BsonDocument getFilterFromLookupRequest(LeaderboardLookup lookup)
        {
            var filterParams = new BsonDocument();
            filterParams["input"] = "$data";
            filterParams["as"] = "data";
            
            var doc = new BsonDocument("$filter", filterParams);
            var totalIn = new BsonArray();
            foreach (var item in lookup.data)
            {
                var key = ("$$data." + item.Key);
                var inParams = new BsonArray();
                inParams.Add(key);

                var inItems = new BsonArray();
                var inValues = new BsonArray();
                var value = (JToken)item.Value;
                if (value.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                {
                    IEnumerable<JToken> subItems = (IEnumerable<JToken>)item.Value;
                    foreach (var subItem in subItems)
                    {
                        JValue subValue = (JValue)subItem;
                        if(subValue.Type == JTokenType.Integer)
                        {
                            inItems.Add(new BsonInt32((int)(long)subValue.Value));
                        } else
                        {
                            inItems.Add(new BsonString(subValue.Value.ToString()));
                        }
                        Console.WriteLine("subType", subValue);
                    }
                }
                else
                {
                    JValue subValue = (JValue)item.Value;
                    if (subValue.Type == JTokenType.Integer)
                    {
                        inItems.Add(new BsonInt32((int)subValue.Value));
                    }
                    else
                    {
                        inItems.Add(new BsonString(subValue.Value.ToString()));
                    }
                }
                inParams.Add(inItems);
                //totalIn.Add(inParams);
                totalIn = inParams;
            }

            var inStmt = new BsonDocument("$in", totalIn);
            var cond = new BsonDocument(inStmt);
            filterParams["cond"] = cond;
            //filterParams["cond"] = new BsonDocument();

            return doc;
        }
        private async Task<BsonDocument> getMatchFromLookupRequest(LeaderboardLookup lookup)
        {
            var matchItems = new BsonDocument
            {

            };

            if (lookup.objectId != null)
            {
                matchItems["_id"] = new BsonObjectId(new ObjectId(lookup.objectId));
            }
            else
            {
                var game = (await gameRepository.Lookup(lookup.gameLookup)).FirstOrDefault();
                matchItems["gameid"] = game.Id;
                if (lookup.baseKey != null)
                {
                    matchItems["baseKey"] = lookup.baseKey;
                }
            }

            return new BsonDocument("$match", matchItems);
        }
        private BsonDocument getPaginationProjection(LeaderboardLookup lookup)
        {
            var sliceItems = new BsonArray();
            sliceItems.Add(new BsonString("$data"));
            sliceItems.Add(new BsonInt32(lookup.pageOffset.Value));
            sliceItems.Add(new BsonInt32(lookup.pageSize.Value));
            var dataItems = new BsonDocument
            (
                new BsonDocument
                {
                    new BsonDocument("$slice", sliceItems)
                }
            );

            return dataItems;
        }
        public async Task<IEnumerable<Leaderboard>> Lookup(LeaderboardLookup lookup)
        {
            Game game = (await gameRepository.Lookup(lookup.gameLookup)).FirstOrDefault();

            var match = await getMatchFromLookupRequest(lookup);
            BsonValue dataFilter = null;
            if(lookup.data == null || lookup.data.Count == 0)
            {
                dataFilter = new BsonInt32(1);
            } else
            {
                dataFilter = getFilterFromLookupRequest(lookup);
            }
            var project = new BsonDocument
            (
                "$project", new BsonDocument
                {
                    new BsonElement("_id", new BsonInt32(1)),
                    new BsonElement("baseKey", new BsonInt32(1)),
                    new BsonElement("gameid", new BsonInt32(1)),
                    new BsonElement("data", dataFilter)
                }
            );

            BsonValue pageFilter = null;
            if (!lookup.pageOffset.HasValue || !lookup.pageSize.HasValue)
            {
                pageFilter = new BsonInt32(1);
            }
            else
            {
                pageFilter = getPaginationProjection(lookup);
            }

            var pagiantionProject = new BsonDocument
            (
                "$project", new BsonDocument
                {
                    new BsonElement("_id", new BsonInt32(1)),
                    new BsonElement("baseKey", new BsonInt32(1)),
                    new BsonElement("gameid", new BsonInt32(1)),
                    new BsonElement("total", new BsonDocument("$size", new BsonString("$data"))),
                    new BsonElement("data", pageFilter)
                }
            );

            BsonDocument[] pipeline = new BsonDocument[] { match, project, pagiantionProject };

            var return_value = new List<Leaderboard>();
            var results = (collection.Aggregate<BsonDocument>(pipeline).ToList<BsonDocument>());
            foreach (var result in results)
            {
                var leaderboard = new Leaderboard();
                leaderboard.game = game;
                leaderboard.total = result.GetValue("total").AsInt32;
                leaderboard.data = Newtonsoft.Json.JsonConvert.DeserializeObject(result.GetValue("data").ToJson()); //stupid fix for weird bson deserialization
                return_value.Add(leaderboard);
            }
            return return_value;
        }

        public Task<Leaderboard> Update(Leaderboard model)
        {
            throw new NotImplementedException();
        }
    }
}
