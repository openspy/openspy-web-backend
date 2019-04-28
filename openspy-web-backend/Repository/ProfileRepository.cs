using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Models;
using System.Text.RegularExpressions;
using CoreWeb.Exception;

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

            bool is_wide = true;

            //user queries
            if (lookup.partnercode.HasValue)
            {
                query = query.Where(b => b.User.Partnercode == lookup.partnercode.Value);
            }
            query = query.Include(b => b.User); //force the user to be loaded

            //profile queries
            if (lookup.id.HasValue)
            {
                is_wide = false;
                query = query.Where(b => b.Id == lookup.id.Value);
            }

            if (lookup.user != null)
            {
                var user = (await this.userRepository.Lookup(lookup.user)).FirstOrDefault();
                if(user != null)
                {
                    query = query.Where(b => b.Userid == user.Id);
                    is_wide = false;
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
                is_wide = false;
            }
            if (lookup.uniquenick != null)
            {
                is_wide = false;
                query = query.Where(b => b.Uniquenick == lookup.uniquenick);
            } else if(lookup.uniquenick_like != null)
            {
                is_wide = false;
                query = query.Where(b => b.Uniquenick.Contains(lookup.uniquenick_like));
            }

            query = query.Where(b => b.Deleted == 0);

            if(is_wide)
            {
                return new List<Profile>();
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
        public async Task<Profile> Update(Profile model)
        {
            //namespaceid 0 cannot have unique nicks
            if (model.Uniquenick != null)
            {
                if (model.Namespaceid == 0 && model.Uniquenick.Length != 0)
                {
                    model.Uniquenick = "";
                }
            }
            model.User = null;
            var tracking = gameTrackerDb.ChangeTracker.QueryTrackingBehavior;
            gameTrackerDb.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            if (await PerformUniqueNickChecks(model) && await PerformNickChecks(model))
            {
                gameTrackerDb.ChangeTracker.QueryTrackingBehavior = tracking;
                var entry = gameTrackerDb.Update<Profile>(model);
                await gameTrackerDb.SaveChangesAsync();
                return entry.Entity;
            }
            gameTrackerDb.ChangeTracker.QueryTrackingBehavior = tracking;
            return null;
        }
        public async Task<Profile> Create(Profile model)
        {
            //namespaceid 0 cannot have unique nicks
            if (model.Uniquenick != null)
            {
                if (model.Namespaceid == 0 && model.Uniquenick.Length != 0)
                {
                    model.Uniquenick = "";
                } else
                {
                    await PerformUniqueNickChecks(model);
                }
            }
            await PerformNickChecks(model);
            model.User = null;
            var entry = gameTrackerDb.Add(model);
            var num_modified = await gameTrackerDb.SaveChangesAsync(true);
            return entry.Entity;
        }
        private bool CheckUniqueNickValid(string uniquenick, int namespaceid)
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
            return true;
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

        //CannotDeleteLastProfileException
        private async Task<bool> PerformNickChecks(Profile value)
        {
            var profileLookup = new ProfileLookup();
            profileLookup.nick = value.Nick;
            profileLookup.uniquenick = value.Uniquenick;
            profileLookup.namespaceid = value.Namespaceid;
            if (value.Userid != 0)
            {
                profileLookup.user = new UserLookup();
                profileLookup.user.id = value.Userid;
            }

            var profile = (await Lookup(profileLookup)).FirstOrDefault();
            if (profile == null)
            {
                return true;
            }

            if (value.Id == profile.Id)
            {
                return true;
            }

            throw new NickInUseException();
        }
        private async Task<bool> PerformUniqueNickChecks(Profile value)
        {
            if (!CheckUniqueNickValid(value.Uniquenick, value.Namespaceid))
            {
                throw new UniqueNickInvalidException();
            }

            /*var profileLookup = new ProfileLookup();
            profileLookup.id = value.Id;
            profileLookup.nick = value.Nick;
            profileLookup.uniquenick = value.Uniquenick;
            profileLookup.namespaceid = value.Namespaceid;
            profileLookup.user = new UserLookup();
            profileLookup.user.id = value.Userid;

            var profile = (await profileRepository.Lookup(profileLookup)).FirstOrDefault();
            if (profile == null)
            {
                throw new NoSuchUserException();
            }*/
            User user = value.User;
            if (user == null)
            {
                var userLookup = new UserLookup();
                userLookup.id = value.Userid;
                user = (await userRepository.Lookup(userLookup)).FirstOrDefault();
                if (user == null)
                {
                    throw new NoSuchUserException();
                }
            }

            var checkData = await CheckUniqueNickInUse(value.Uniquenick, value.Namespaceid, user.Partnercode);
            if (checkData.Item1)
            {
                if (checkData.Item2 != value.Id)
                    throw new UniqueNickInUseException(checkData.Item2, checkData.Item3);
            }
            return true;
        }
    }
}
