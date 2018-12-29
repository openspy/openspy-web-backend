using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Models;

namespace CoreWeb.Repository
{
    public class GroupRepository : IRepository<Group, GroupLookup>
    {
        private GamemasterDBContext gameMasterDb;
        public GroupRepository(GamemasterDBContext gameMasterDb)
        {
            this.gameMasterDb = gameMasterDb;
        }
        public async Task<IEnumerable<Group>> Lookup(GroupLookup lookup)
        {
            if(lookup.id.HasValue)
            {
                var results = await gameMasterDb.Group.Where(b => b.Groupid == lookup.id.Value).ToListAsync();
                return results;
            } else if(lookup.gameid.HasValue)
            {
                var results = await gameMasterDb.Group.Where(b => b.Gameid == lookup.gameid).ToListAsync();
                return results;
            }
            return null;
        }
        public Task<bool> Delete(GroupLookup lookup)
        {
            return Task.Run(async () =>
            {
                var groups = (await Lookup(lookup)).ToList();
                foreach (var group in groups)
                {
                    gameMasterDb.Remove<Group>(group);
                }
                var num_modified = await gameMasterDb.SaveChangesAsync();
                return groups.Count > 0 && num_modified > 0;
            });
        }
        public Task<Group> Update(Group model)
        {
            return Task.Run(async () =>
            {
                var entry = gameMasterDb.Update<Group>(model);
                await gameMasterDb.SaveChangesAsync();
                return entry.Entity;
            });
        }
        public async Task<Group> Create(Group model)
        {
            var entry = await gameMasterDb.AddAsync<Group>(model);
            var num_modified = await gameMasterDb.SaveChangesAsync();
            return entry.Entity;
        }
    }
}
