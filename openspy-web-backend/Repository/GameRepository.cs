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
        public GameRepository(GamemasterDBContext gameMasterDb)
        {
            this.gameMasterDb = gameMasterDb;
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
            }
            return null;
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
    }
}
