using CoreWeb.Database;
using CoreWeb.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Repository
{
    public class PersistDataRepository : IRepository<PersistData, PersistDataLookup>
    {
        private GameTrackerDBContext gameTrackerDb;
        private IRepository<User, UserLookup> userRepository;
        private IRepository<Profile, ProfileLookup> profileRepository;
        private IRepository<Game, GameLookup> gameRepository;
        public PersistDataRepository(GameTrackerDBContext gameTrackerDb, IRepository<User, UserLookup> userRepository, IRepository<Profile, ProfileLookup> profileRepository, IRepository<Game, GameLookup> gameRepository)
        {
            this.gameTrackerDb = gameTrackerDb;
            this.userRepository = userRepository;
            this.profileRepository = profileRepository;
            this.gameRepository = gameRepository;
        }
        public async Task<PersistData> Create(PersistData model)
        {
            model.Modified = DateTime.UtcNow;
            var entry = await gameTrackerDb.AddAsync<PersistData>(model);
            var num_modified = await gameTrackerDb.SaveChangesAsync();
            return entry.Entity;
        }

        public Task<bool> Delete(PersistDataLookup lookup)
        {
            return Task.Run(async () =>
            {
                var keys = (await Lookup(lookup)).ToList();
                foreach (var key in keys)
                {
                    gameTrackerDb.Remove<PersistData>(key);
                }
                var num_modified = await gameTrackerDb.SaveChangesAsync();
                return keys.Count > 0 && num_modified > 0;
            });
        }

        public async Task<IEnumerable<PersistData>> Lookup(PersistDataLookup lookup)
        {
            var game = (await gameRepository.Lookup(lookup.gameLookup)).FirstOrDefault();
            var profile = (await profileRepository.Lookup(lookup.profileLookup)).FirstOrDefault();
            var query = gameTrackerDb.PersistData as IQueryable<PersistData>;

            query = query.Where(s => s.Profileid == profile.Id && s.Gameid == game.Id &&  s.DataIndex == lookup.DataIndex && s.PersistType == lookup.PersistType);
            if(lookup.modifiedSince.HasValue) {
                query = query.Where(s => s.Modified >= lookup.modifiedSince.Value);
            }
            return await query.ToListAsync();
        }

        public Task<PersistData> Update(PersistData model)
        {
            return Task.Run(async () =>
            {
                model.Modified = DateTime.UtcNow;
                var entry = gameTrackerDb.Update<PersistData>(model);
                await gameTrackerDb.SaveChangesAsync();
                return entry.Entity;
            });
        }
    }
}
