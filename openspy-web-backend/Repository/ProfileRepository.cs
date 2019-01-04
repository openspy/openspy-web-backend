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
            var query = gameTrackerDb.Profile as IQueryable<Profile>;
            if (lookup.id.HasValue)
            {
                query = query.Where(b => b.Id == lookup.id.Value);
            }
            if(lookup.userId.HasValue)
            {
                query = query.Where(b => b.Userid == lookup.userId.Value);
            }
            if (lookup.namespaceid.HasValue)
            {
                query = query.Where(b => b.Namespaceid == lookup.namespaceid.Value);
            }
            if (lookup.uniquenick != null)
            {
                query = query.Where(b => b.Uniquenick == lookup.uniquenick);
            }
            if(lookup.partnercode.HasValue)
            {
                query = query.Where(b => b.User.Partnercode == lookup.partnercode.Value);
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
            var entry = gameTrackerDb.Add(model);
            var num_modified = await gameTrackerDb.SaveChangesAsync(true);
            return entry.Entity;
        }

        public async Task<ValueTuple<bool, int, int>> CheckUniqueNickInUse(string uniquenick, int? namespaceid, int ?partnercode)
        {
            ValueTuple<bool, int, int> ret = new ValueTuple<bool, int, int>(false, 0, 0);
            if (!namespaceid.HasValue || namespaceid.Value == 0)
            {
                return ret;
            }

            ProfileLookup profileLookup = new ProfileLookup();
            profileLookup.uniquenick = uniquenick;
            profileLookup.namespaceid = namespaceid;
            profileLookup.partnercode = partnercode;
            var profile = (await Lookup(profileLookup)).FirstOrDefault();

            if (profile != null)
            {
                UserLookup userLookup = new UserLookup();
                ret.Item1 = true;
                ret.Item2 = profile.Id;
                ret.Item3 = profile.Userid;
            }
            return ret;
        }
    }
}
