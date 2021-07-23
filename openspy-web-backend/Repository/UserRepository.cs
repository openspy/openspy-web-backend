using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Models;

namespace CoreWeb.Repository
{
    public class UserRepository : IRepository<User, UserLookup>
    {
        private const int REDIS_SESSION_DB = 3;
        private readonly int PARTNERID_GAMESPY = 0;
        private readonly int PARTNERID_IGN = 10;
        private readonly int PARTNERID_EA = 20;
        private GameTrackerDBContext gameTrackerDb;
        private SessionCacheDatabase sessionCache;
        public UserRepository(GameTrackerDBContext gameTrackerDb, SessionCacheDatabase sessionCache)
        {
            this.gameTrackerDb = gameTrackerDb;
            this.sessionCache = sessionCache;
        }
        public async Task<IEnumerable<User>> Lookup(UserLookup lookup)
        {
            bool is_wide = true;
            var query = gameTrackerDb.User as IQueryable<User>;
            if (lookup.id.HasValue)
            {
                query = query.Where(b => b.Id == lookup.id.Value);
                is_wide = false;
            }
            if (lookup.email != null)
            {
                query = query.Where(b => b.Email.Equals(lookup.email));
                is_wide = false;
            }
            if (is_wide) //too many results would be found... return no results
            {
                return new List<User>();
            }
            if(lookup.partnercode.HasValue)
            {
                query = query.Where(b => b.Partnercode == lookup.partnercode.Value);
            }
            query = query.Where(b => b.Deleted == false);
            return await query.ToListAsync();
        }
        public Task<bool> Delete(UserLookup lookup)
        {
            return Task.Run(async () =>
            {
                var users = (await Lookup(lookup)).ToList();
                foreach (var user in users)
                {
                    user.Deleted = true;
                    gameTrackerDb.Update<User>(user);
                }
                var num_modified = await gameTrackerDb.SaveChangesAsync();
                return users.Count > 0 && num_modified > 0;
            });
        }
         public async Task<bool> UpdatePassword(int userId, string password) {
            UserLookup userLookup = new UserLookup();
            userLookup.id = userId;
            User userModel = (await Lookup(userLookup)).FirstOrDefault();

            userModel.Password = password;

            var entry = gameTrackerDb.Update<User>(userModel);
            var saveResult = await gameTrackerDb.SaveChangesAsync();
            return saveResult > 0;
         }
        public async Task<User> Update(User model)
        {
            UserLookup userLookup = new UserLookup();
            userLookup.id = model.Id;
            User userModel = (await Lookup(userLookup)).FirstOrDefault();

            userModel.Copy(model);

            var entry = gameTrackerDb.Update<User>(userModel);
            await gameTrackerDb.SaveChangesAsync();
            return entry.Entity;
        }
        public async Task<User> Create(User model)
        {
            var entry = await gameTrackerDb.AddAsync<User>(model);
            var num_modified = await gameTrackerDb.SaveChangesAsync(true);
            return entry.Entity;
        }
        public async Task<bool> SendEmailVerification(User model) {
            var verify_key = Guid.NewGuid().ToString();
            var store_key = "verify_" + model.Id;
            var db = sessionCache.GetDatabase();
            if(db.KeyExists(store_key)) return false;
            var result = db.StringSet(store_key, verify_key);
            db.KeyExpire(store_key, TimeSpan.FromHours(6));

            model.EmailVerified = false;
            var entry = gameTrackerDb.Update<User>(model);
            await gameTrackerDb.SaveChangesAsync();
            return true;    
        }
        public async Task<bool> PerformEmailVerification(User model, string verification_key) {
            var store_key = "verify_" + model.Id;
            var db = sessionCache.GetDatabase();
            var result = db.StringGet(store_key);
            if(!db.KeyExists(store_key)) return false;
            if(result.CompareTo(verification_key) == 0) {
                model.EmailVerified = true;
                var entry = gameTrackerDb.Update<User>(model);
                await gameTrackerDb.SaveChangesAsync();
                db.KeyDelete(store_key);
                return true;
            }
            return false;
        }
        public async Task<bool> SendPasswordReset(User model) {
            var verify_key = Guid.NewGuid().ToString();
            var store_key = "reset_" + model.Id;
            var db = sessionCache.GetDatabase();
            if(db.KeyExists(store_key)) return false;
            var result = db.StringSet(store_key, verify_key);            
            db.KeyExpire(store_key, TimeSpan.FromHours(6));

            model.EmailVerified = false;
            var entry = gameTrackerDb.Update<User>(model);
            await gameTrackerDb.SaveChangesAsync();
            return true;    
        }
        public async Task<bool> PerformPasswordReset(User model, string verification_key, string password) {
            var store_key = "reset_" + model.Id;
            var db = sessionCache.GetDatabase();
            var result = db.StringGet(store_key);
            if(!db.KeyExists(store_key)) return false;
            if(result.CompareTo(verification_key) == 0) {
                model.Password = password;
                var entry = gameTrackerDb.Update<User>(model);
                await gameTrackerDb.SaveChangesAsync();
                db.KeyDelete(store_key);
                return true;
            }
            return false;
        }
    }
}
