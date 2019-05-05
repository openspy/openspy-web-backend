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

    public class PlayerProgressRepository : IRepository<PlayerProgress, PlayerProgressLookup>
    {
        private IRepository<Profile, ProfileLookup> profileRepository;
        private IRepository<Game, GameLookup> gameRepository;
        private IMongoCollection<BsonDocument> collection;

        public PlayerProgressRepository(IRepository<Profile, ProfileLookup> profileRepository, IRepository<Game, GameLookup> gameRepository, ISnapShotDBContext snapshotDb)
        {
            this.profileRepository = profileRepository;
            this.gameRepository = gameRepository;

            var db = snapshotDb.GetDatabase();
            collection = db.GetCollection<BsonDocument>("player_progress");
        }
        public Task<PlayerProgress> Create(PlayerProgress model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(PlayerProgressLookup lookup)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<PlayerProgress>> Lookup(PlayerProgressLookup lookup)
        {
            Game game = null;
            Profile profile = null;

            var searchRequest = new BsonDocument
            {
            };

            if(lookup.objectId != null)
            {
                searchRequest["_id"] = new BsonObjectId(new ObjectId(lookup.objectId));
            } else
            {
                profile = (await profileRepository.Lookup(lookup.profileLookup)).FirstOrDefault();
                game = (await gameRepository.Lookup(lookup.gameLookup)).FirstOrDefault();
                searchRequest["gameid"] = game.Id;
                searchRequest["profileid"] = profile.Id;

                if (lookup.pageKey != null)
                {
                    searchRequest["pageKey"] = lookup.pageKey;
                }
            }
            var return_value = new List<PlayerProgress>();
            var results = (await collection.FindAsync(searchRequest)).ToList();
            foreach(var result in results)
            {
                var progress = new PlayerProgress();

                if(!result["profileid"].IsBsonNull && result["profileid"].AsInt32 != 0)
                {
                    var profileLookup = new ProfileLookup();
                    profileLookup.id = result["profileid"].AsInt32;
                    profile = (await profileRepository.Lookup(profileLookup)).FirstOrDefault();
                } else
                {
                    profile = null;
                }

                if (!result["gameid"].IsBsonNull || result["gameid"].AsInt32 != 0)
                {
                    var gameLookup = new GameLookup();
                    gameLookup.id = result["gameid"].AsInt32;
                    game = (await gameRepository.Lookup(gameLookup)).FirstOrDefault();
                }else
                {
                    game = null;
                }   

                progress.game = game;
                progress.profile = profile;
                progress.data = Newtonsoft.Json.JsonConvert.DeserializeObject(result.GetValue("data").ToJson()); //stupid fix for weird bson deserialization

                if(result.Contains("modified"))
                    progress.modified = (decimal)result["modified"].AsDouble;
                return_value.Add(progress);
            }
            return return_value;
        }

        public Task<PlayerProgress> Update(PlayerProgress model)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetData(PlayerProgressSet lookup)
        {
            var searchRequest = new BsonDocument
            {
            };

            var profile = (await profileRepository.Lookup(lookup.profileLookup)).FirstOrDefault();
            var game = (await gameRepository.Lookup(lookup.gameLookup)).FirstOrDefault();
            searchRequest["gameid"] = game.Id;
            searchRequest["profileid"] = profile.Id;

            if (lookup.pageKey != null)
            {
                searchRequest["pageKey"] = lookup.pageKey;
            }

            var updateData = new BsonDocument {

            };
            foreach(var item in lookup.SetData) {
                updateData[item.Key.ToString()] = item.Value.ToString();
            }
            var updateRequest = new BsonDocument (
                "$set", updateData
            );
            var result = (await collection.UpdateOneAsync(searchRequest, updateRequest));
            return result.IsAcknowledged && result.IsModifiedCountAvailable && result.ModifiedCount > 0;
        }
    }
}
