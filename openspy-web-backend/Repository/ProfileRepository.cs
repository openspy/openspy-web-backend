using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Models;
using System.Text.RegularExpressions;

namespace CoreWeb.Repository
{
    public class ProfileRepository : IRepository<Profile, ProfileLookup>
    {
        private readonly int NAMESPACEID_GAMESPY = 1;
        private readonly int NAMESPACEID_IGN = 15;
        private GameTrackerDBContext gameTrackerDb;
        private IRepository<User, UserLookup> userRepository;
        public ProfileRepository(GameTrackerDBContext gameTrackerDb, IRepository<User, UserLookup> userRepository)
        {
            this.gameTrackerDb = gameTrackerDb;
            this.userRepository = userRepository;
        }
        public async Task<IEnumerable<Profile>> Lookup(ProfileLookup lookup)
        {
            var query = gameTrackerDb.Profile as IQueryable<Profile>;
            
            //user queries
            if (lookup.partnercode.HasValue)
            {
                query = query.Where(b => b.User.Partnercode == lookup.partnercode.Value);
            }
            query = query.Include(b => b.User); //force the user to be loaded

            //profile queries
            if (lookup.id.HasValue)
            {
                query = query.Where(b => b.Id == lookup.id.Value);
            }

            if (lookup.user != null)
            {
                var user = (await this.userRepository.Lookup(lookup.user)).FirstOrDefault();
                if(user != null)
                {
                    query = query.Where(b => b.Userid == user.Id);
                }
            }
            if (lookup.namespaceid.HasValue)
            {
                query = query.Where(b => b.Namespaceid == lookup.namespaceid.Value);
            }
            if(lookup.namespaceids != null)
            {
                query = query.Where(b => lookup.namespaceids.Contains(b.Namespaceid));
            }
            if (lookup.nick != null)
            {
                query = query.Where(b => b.Nick == lookup.nick);
            }
            if (lookup.uniquenick != null)
            {
                query = query.Where(b => b.Uniquenick == lookup.uniquenick);
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
        public bool CheckUniqueNickValid(string uniquenick, int namespaceid)
        {
            if(namespaceid == NAMESPACEID_IGN)
            {
                var allowed_chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_.";
                foreach (var ch in uniquenick)
                {
                    if (!allowed_chars.Contains(ch)) return false;
                }
                return true;
            }
            switch(uniquenick[0])
            {
                case '@':
                case '+':
                case '#':
                case ':':
                    return false;
            }
            foreach(var ch in uniquenick)
            {
                if(ch <= 34)
                {
                    return false;
                }
                if(ch >= 126)
                {
                    return false;
                }
                if(ch == '\\' || ch ==',')
                {
                    return false;
                }
            }

            //var striped_name = Regex.Replace(uniquenick, @"[^A-Za-z0-9]+", "");
            //TODO: check list of bad uniquenicks
            return false;
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
