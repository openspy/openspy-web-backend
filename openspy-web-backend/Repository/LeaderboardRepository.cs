using CoreWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CoreWeb.Repository
{
    public class Leaderboard
    {
        public string pageKey;
        public object data;
        public Game game;
    };
    public class LeaderboardLookup
    {
        public string objectId;
        public GameLookup gameLookup;
        public string pageKey;
        public string baseKey;
        public Dictionary<string, object> data; //data matches
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
        /*
db.getCollection('leaderboards').aggregate([{$match: {gameid: 1324, "baseKey": "overallscore", "data.pid" : {$in: [11141,1400163188]}}}
,{
    $group: { _id: "$baseKey", "data": {$push: "$data"}}
}
, {$unwind: "$data"}
,{$project: {_id: 1, data: {$filter: {input: "$data", as: "data", cond: {$in: ["$$data.pid", [11141, 11176,1400163188]]}}}}},
{$unwind: "$data"},
{
    $group: { _id: "$_id", "data": {$push: "$data"}}
}
])
         */
        private BsonDocument getFilterFromLookupRequest(LeaderboardLookup lookup)
        {
            var filterParams = new BsonDocument();
            filterParams["input"] = "$data";
            filterParams["as"] = "data";

            var inParams = new BsonArray();
            inParams.Add("$$data.pid");

            var inPids = new BsonArray();
            inPids.Add(new BsonInt32(11141));
            inPids.Add(new BsonInt32(11176));
            inPids.Add(new BsonInt32(1400163188));
            inParams.Add(inPids);

            var inStmt = new BsonDocument("$in", inParams);
            var cond = new BsonDocument(inStmt);
            
            var doc = new BsonDocument("$filter", filterParams);

            filterParams["cond"] = cond;

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
                if (lookup.pageKey != null)
                {
                    matchItems["pageKey"] = lookup.pageKey;
                }
                if (lookup.baseKey != null)
                {
                    matchItems["baseKey"] = lookup.baseKey;
                }
            }

            return new BsonDocument("$match", matchItems);
        }
        public async Task<IEnumerable<Leaderboard>> Lookup(LeaderboardLookup lookup)
        {
            Game game = null;

            var match = await getMatchFromLookupRequest(lookup);
            
            var firstGroup = new BsonDocument
            (
                "$group", new BsonDocument(
                    new BsonElement("_id", new BsonString("$baseKey")),
                    new BsonElement("data", new BsonDocument(
                     new BsonElement("$push", new BsonString("$data"))
                    )
                ))
            );
            var dataUnwind = new BsonDocument
            (
                "$unwind", new BsonString("$data")
            );


            var project = new BsonDocument
            (
                "$project", new BsonDocument
                {
                    new BsonElement("_id", new BsonInt32(1)),
                    new BsonElement("data", getFilterFromLookupRequest(lookup))
                }
            );

            var secondGroup = new BsonDocument
            ("$group", new BsonDocument(
                    new BsonElement("_id", new BsonString("_id")),
                    new BsonElement("data", new BsonDocument(
                     new BsonElement("$push", new BsonString("$data"))
                    )
                )));


            BsonDocument[] pipeline = new BsonDocument[] { match, firstGroup, dataUnwind, project, dataUnwind, secondGroup };

            //var pipeline = new[] { match, firstGroup, dataUnwind, project, secondGroup };
            var return_value = new List<Leaderboard>();
            var results = (collection.Aggregate<BsonDocument>(pipeline).ToList<BsonDocument>());
            foreach (var result in results)
            {
                var leaderboard = new Leaderboard();
                leaderboard.game = game;
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
