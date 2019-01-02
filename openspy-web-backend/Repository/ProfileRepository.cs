using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Models;

namespace CoreWeb.Repository
{
    public class ProfileRepository : IRepository<Profile, ProfileLookup>
    {
        private GameTrackerDBContext gameTrackerDb;
        public ProfileRepository(GameTrackerDBContext gameTrackerDb)
        {
            this.gameTrackerDb = gameTrackerDb;
        }
        public async Task<IEnumerable<Profile>> Lookup(ProfileLookup lookup)
        {
            var query = gameTrackerDb.Profile;
            if (lookup.id.HasValue)
            {
                query.Where(b => b.Id == lookup.id.Value);
            }
            if(lookup.userId.HasValue)
            {
                query.Where(b => b.Userid == lookup.userId.Value);
            }
            if (lookup.namespaceid.HasValue)
            {
                query.Where(b => b.Namespaceid == lookup.namespaceid.Value);
            }
            if (lookup.uniquenick != null)
            {
                query.Where(b => b.Uniquenick == lookup.uniquenick);
            }
            if(lookup.partnercode.HasValue)
            {
                query.Where(b => b.User.Partnercode == lookup.partnercode.Value);
            }
            return await query.ToListAsync();
        }
        public Task<bool> Delete(ProfileLookup lookup)
        {
            return Task.Run(async () =>
            {
                var profiles = (await Lookup(lookup)).ToList();
                foreach (var profile in profiles)
                {
                    gameTrackerDb.Remove<Profile>(profile);
                }
                var num_modified = await gameTrackerDb.SaveChangesAsync();
                return profiles.Count > 0 && num_modified > 0;
            });
        }
        public Task<Profile> Update(Profile model)
        {
            return Task.Run(async () =>
            {
                var entry = gameTrackerDb.Update<Profile>(model);
                await gameTrackerDb.SaveChangesAsync();
                return entry.Entity;
            });
        }
        public async Task<Profile> Create(Profile model)
        {
            var entry = await gameTrackerDb.AddAsync<Profile>(model);
            var num_modified = await gameTrackerDb.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<ValueTuple<bool, int>> CheckUniqueNickInUse(string uniquenick, int? namespaceid, int ?partnercode)
        {

            ValueTuple<bool, int> ret = new ValueTuple<bool, int>(false, 0);
            if (!namespaceid.HasValue || namespaceid.Value == 0)
            {
                ret.Item1 = false;
                ret.Item2 = 0;
                return ret;
            }

            ProfileLookup profileLookup = new ProfileLookup();
            profileLookup.uniquenick = uniquenick;
            profileLookup.namespaceid = namespaceid;
            profileLookup.partnercode = partnercode;
            var profile = (await Lookup(profileLookup)).First();

            if(profile != null)
            {
                ret.Item1 = true;
                ret.Item2 = profile.Id;
            } else
            {
                ret.Item1 = false;
                ret.Item2 = 0;
            }
            return ret;
        }
    }
}
