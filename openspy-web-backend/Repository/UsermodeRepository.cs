using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Models;

namespace CoreWeb.Repository
{
    public class UsermodeRepository : IRepository<UsermodeRecord, UsermodeLookup>
    {
        private PeerchatDBContext peerChatDb;
        public UsermodeRepository(PeerchatDBContext peerChatDb)
        {
            this.peerChatDb = peerChatDb;
        }
        public async Task<IEnumerable<UsermodeRecord>> Lookup(UsermodeLookup lookup)
        {
            var query = peerChatDb.Usermode as IQueryable<UsermodeRecord>;
            if (lookup.Id.HasValue)
            {
                query = peerChatDb.Usermode.Where(b => b.Id == lookup.Id.Value);
            }
            
            if (lookup.channelmask != null) {
                if(lookup.channelmask.Contains("*")) //wildcard search
                {
                    var mask = lookup.channelmask.Replace("*", "%");
                    query = query.Where(b => EF.Functions.Like(b.channelmask, mask));
                } else
                {
                    query = query.Where(b => b.channelmask == lookup.channelmask);
                }
            }
            if(lookup.profileid.HasValue)
            {
                query = query.Where(b => b.profileid == lookup.profileid.Value);
            }
            if(lookup.hostmask != null)
            {
                query = query.Where(b => b.hostmask == lookup.hostmask);
            }
            query = query.Where(b => b.expiresAt == null || b.expiresAt > DateTime.UtcNow);
            return await query.ToListAsync();
        }
        public Task<bool> Delete(UsermodeLookup lookup)
        {
            return Task.Run(async () =>
            {
                var usermodes = (await Lookup(lookup)).ToList();
                foreach (var usermode in usermodes)
                {
                    peerChatDb.Remove<UsermodeRecord>(usermode);
                }
                var num_modified = await peerChatDb.SaveChangesAsync();
                return usermodes.Count > 0 && num_modified > 0;
            });
        }
        public Task<UsermodeRecord> Update(UsermodeRecord model)
        {
            return Task.Run(async () =>
            {
                var entry = peerChatDb.Update<UsermodeRecord>(model);
                await peerChatDb.SaveChangesAsync();
                return entry.Entity;
            });
        }
        public async Task<UsermodeRecord> Create(UsermodeRecord model)
        {
            model.setAt = DateTime.UtcNow;
            var entry = await peerChatDb.AddAsync<UsermodeRecord>(model);
            var num_modified = await peerChatDb.SaveChangesAsync();
            return entry.Entity;
        }
    }
}
