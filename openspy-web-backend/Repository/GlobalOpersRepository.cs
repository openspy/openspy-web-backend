using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Models;

namespace CoreWeb.Repository
{
    public class GlobalOpersRepository : IRepository<GlobalOpersRecord, GlobalOpersLookup>
    {
        private PeerchatDBContext peerChatDb;
        public GlobalOpersRepository(PeerchatDBContext peerChatDb)
        {
            this.peerChatDb = peerChatDb;
        }
        public async Task<IEnumerable<GlobalOpersRecord>> Lookup(GlobalOpersLookup lookup)
        {
            var query = peerChatDb.GlobalOpers as IQueryable<GlobalOpersRecord>;

            query = query.Where(b => b.profileid == lookup.profileid);

            return await query.ToListAsync();
        }
        public Task<bool> Delete(GlobalOpersLookup lookup)
        {
            return Task.Run(async () =>
            {
                var users = (await Lookup(lookup)).ToList();
                foreach (var user in users)
                {
                    peerChatDb.Remove<GlobalOpersRecord>(user);
                }
                var num_modified = await peerChatDb.SaveChangesAsync();
                return users.Count > 0 && num_modified > 0;
            });
        }
        public Task<GlobalOpersRecord> Update(GlobalOpersRecord model)
        {
            return Task.Run(async () =>
            {
                var entry = peerChatDb.Update<GlobalOpersRecord>(model);
                await peerChatDb.SaveChangesAsync();
                return entry.Entity;
            });
        }
        public async Task<GlobalOpersRecord> Create(GlobalOpersRecord model)
        {
            var entry = await peerChatDb.AddAsync<GlobalOpersRecord>(model);
            var num_modified = await peerChatDb.SaveChangesAsync(true);
            return entry.Entity;
        }
    }
}
