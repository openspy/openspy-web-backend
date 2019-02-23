using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Models;

namespace CoreWeb.Repository
{
    public class GameRepository : IRepository<Game, GameLookup>
    {
        private GamemasterDBContext gameMasterDb;
        private GameCacheDatabase gameCacheDatabase;
        public GameRepository(GamemasterDBContext gameMasterDb, GameCacheDatabase gameCacheDatabase)
        {
            this.gameMasterDb = gameMasterDb;
            this.gameCacheDatabase = gameCacheDatabase;
        }
        public async Task<IEnumerable<Game>> Lookup(GameLookup lookup)
        {
            if(lookup.id.HasValue)
            {
                var results = await gameMasterDb.Game.Where(b => b.Id == lookup.id.Value).ToListAsync();
                return results;
            } else if(lookup.gamename != null)
            {
                var results = await gameMasterDb.Game.Where(b => b.Gamename == lookup.gamename).ToListAsync();
                return results;
            } else
            {
                var results = await gameMasterDb.Game.ToListAsync();
                return results;
            }
        }
        public Task<bool> Delete(GameLookup lookup)
        {
            return Task.Run(async () =>
            {
                var games = (await Lookup(lookup)).ToList();
                foreach (var game in games)
                {
                    gameMasterDb.Remove<Game>(game);
                }
                var num_modified = await gameMasterDb.SaveChangesAsync();
                return games.Count > 0 && num_modified > 0;
            });
        }
        public Task<Game> Update(Game model)
        {
            return Task.Run(async () =>
            {
                var entry = gameMasterDb.Update<Game>(model);
                await gameMasterDb.SaveChangesAsync();
                return entry.Entity;
            });
        }
        public async Task<Game> Create(Game model)
        {
            var entry = await gameMasterDb.AddAsync<Game>(model);
            var num_modified = await gameMasterDb.SaveChangesAsync();
            return entry.Entity;
        }
        public Task SyncToRedis()
        {
            return Task.Run(async () =>
            {

                var db = gameCacheDatabase.GetDatabase();
                var lookup = new GameLookup();
                var games = await Lookup(lookup);

                gameCacheDatabase.FlushDatabase();

                foreach (var game in games)
                {
                    var game_key = game.Gamename + ":" + game.Id.ToString();
                    db.HashSet(game_key, "gameid", game.Id.ToString());
                    db.HashSet(game_key, "description", game.Description);
                    db.HashSet(game_key, "gamename", game.Gamename);
                    db.HashSet(game_key, "secretkey", game.Secretkey);
                    db.HashSet(game_key, "queryport", game.Queryport.ToString());
                    db.HashSet(game_key, "disabledservices", game.Disabledservices.ToString());
                    db.HashSet(game_key, "backendflags", game.Backendflags.ToString());
                    db.StringSet(game.Gamename, game_key);
                    db.SetAdd("gameid_" + game.Id.ToString(), game_key);
                }
            });
        }
    }
}
