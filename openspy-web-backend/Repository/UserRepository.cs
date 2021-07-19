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
        private readonly int PARTNERID_GAMESPY = 0;
        private readonly int PARTNERID_IGN = 10;
        private readonly int PARTNERID_EA = 20;
        private GameTrackerDBContext gameTrackerDb;
        public UserRepository(GameTrackerDBContext gameTrackerDb)
        {
            this.gameTrackerDb = gameTrackerDb;
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
                    gameTrackerDb.Remove<User>(user);
                }
                var num_modified = await gameTrackerDb.SaveChangesAsync();
                return users.Count > 0 && num_modified > 0;
            });
        }
        public Task<User> Update(User model)
        {
            return Task.Run(async () =>
            {
                UserLookup userLookup = new UserLookup();
                userLookup.id = model.Id;
                User userModel = (await Lookup(userLookup)).FirstOrDefault();

                userModel.Copy(model);

                var entry = gameTrackerDb.Update<User>(userModel);
                await gameTrackerDb.SaveChangesAsync();
                return entry.Entity;
            });
        }
        public async Task<User> Create(User model)
        {
            var entry = await gameTrackerDb.AddAsync<User>(model);
            var num_modified = await gameTrackerDb.SaveChangesAsync(true);
            return entry.Entity;
        }
    }
}
