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

        public async Task<IEnumerable<Leaderboard>> Lookup(LeaderboardLookup lookup)
        {
            Game game = null;

            var searchRequest = new BsonDocument
            {
            };

            if (lookup.objectId != null)
            {
                searchRequest["_id"] = new BsonObjectId(new ObjectId(lookup.objectId));
            }
            else
            {
                game = (await gameRepository.Lookup(lookup.gameLookup)).FirstOrDefault();
                searchRequest["gameid"] = game.Id;
            }
            var return_value = new List<Leaderboard>();
            var results = (await collection.FindAsync(searchRequest)).ToList();
            foreach (var result in results)
            {
                var leaderboard = new Leaderboard();
                var gameLookup = new GameLookup();
                gameLookup.id = result["gameid"].ToInt32();
                leaderboard.game = (await gameRepository.Lookup(lookup.gameLookup)).FirstOrDefault();
                leaderboard.data = result.GetValue(lookup.pageKey);
                leaderboard.pageKey = lookup.pageKey;
                return_value.Add(leaderboard);
                return return_value;
            }
            return null;
        }

        public Task<Leaderboard> Update(Leaderboard model)
        {
            throw new NotImplementedException();
        }
    }
}
