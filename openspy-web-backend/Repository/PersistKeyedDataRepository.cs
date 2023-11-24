using CoreWeb.Database;
using CoreWeb.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Repository
{
    public class PersistKeyedDataRepository : IRepository<PersistKeyedData, PersistKeyedDataLookup>
    {
        private GameTrackerDBContext gameTrackerDb;
        private IRepository<User, UserLookup> userRepository;
        private IRepository<Profile, ProfileLookup> profileRepository;
        private IRepository<Game, GameLookup> gameRepository;
        public PersistKeyedDataRepository(GameTrackerDBContext gameTrackerDb, IRepository<User, UserLookup> userRepository, IRepository<Profile, ProfileLookup> profileRepository, IRepository<Game, GameLookup> gameRepository)
        {
            this.gameTrackerDb = gameTrackerDb;
            this.userRepository = userRepository;
            this.profileRepository = profileRepository;
            this.gameRepository = gameRepository;
        }
        public async Task<PersistKeyedData> Create(PersistKeyedData model)
        {
            var entry = await gameTrackerDb.AddAsync<PersistKeyedData>(model);
            var num_modified = await gameTrackerDb.SaveChangesAsync();
            return entry.Entity;
        }

        public Task<bool> Delete(PersistKeyedDataLookup lookup)
        {
            return Task.Run(async () =>
            {
                var keys = (await Lookup(lookup)).ToList();
                foreach (var key in keys)
                {
                    gameTrackerDb.Remove<PersistKeyedData>(key);
                }
                var num_modified = await gameTrackerDb.SaveChangesAsync();
                return keys.Count > 0 && num_modified > 0;
            });
        }

        public async Task<IEnumerable<PersistKeyedData>> Lookup(PersistKeyedDataLookup lookup)
        {
            var game = (await gameRepository.Lookup(lookup.gameLookup)).FirstOrDefault();
            var profile = (await profileRepository.Lookup(lookup.profileLookup)).FirstOrDefault();
            var query = gameTrackerDb.PersistKeyedData as IQueryable<PersistKeyedData>;

            if (lookup.keys != null && lookup.keys.Count > 0) {
                query = query.Where(s => s.Profileid == profile.Id && s.Gameid == game.Id && lookup.keys.Contains(s.KeyName) && s.DataIndex == lookup.dataIndex && s.PersistType == lookup.persistType);
            } else {
                query = query.Where(s => s.Profileid == profile.Id && s.Gameid == game.Id && s.DataIndex == lookup.dataIndex && s.PersistType == lookup.persistType);
            }
            
            if(lookup.modifiedSince.HasValue) {
                query = query.Where(s => s.Modified >= lookup.modifiedSince.Value);
            }
            return await query.ToListAsync();
        }

        public Task<PersistKeyedData> Update(PersistKeyedData model)
        {
            return Task.Run(async () =>
            {
                var entry = gameTrackerDb.Update<PersistKeyedData>(model);
                await gameTrackerDb.SaveChangesAsync();
                return entry.Entity;
            });
        }
    }
}
