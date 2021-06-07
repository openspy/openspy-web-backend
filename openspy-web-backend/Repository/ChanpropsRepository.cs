using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Models;
using StackExchange.Redis;
using RabbitMQ.Client;
namespace CoreWeb.Repository
{
    public class ChanpropsRepository : IRepository<ChanpropsRecord, ChanpropsLookup>
    {
        private PeerchatDBContext peerChatDb;
        private PeerchatCacheDatabase peerChatCacheDb;
        private IMQConnectionFactory connectionFactory;
        public ChanpropsRepository(PeerchatDBContext peerChatDb, PeerchatCacheDatabase peerChatCacheDb, IMQConnectionFactory connectionFactory)
        {
            this.peerChatDb = peerChatDb;
            this.peerChatCacheDb = peerChatCacheDb;
            this.connectionFactory = connectionFactory;
        }
        public async Task<ChanpropsRecord> Create(ChanpropsRecord model)
        {
            model.setAt = DateTime.UtcNow;
            var entry = await peerChatDb.AddAsync<ChanpropsRecord>(model);
            var num_modified = await peerChatDb.SaveChangesAsync();            
            return entry.Entity;
        }

        public Task<bool> Delete(ChanpropsLookup lookup)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ChanpropsRecord>> Lookup(ChanpropsLookup lookup)
        {
            var query = peerChatDb.Chanprops as IQueryable<ChanpropsRecord>;
            if (lookup.Id.HasValue)
            {
                query = peerChatDb.Chanprops.Where(b => b.Id == lookup.Id.Value);
            } else if(lookup.channelmask != null && !lookup.channelmask.Contains("*")) {
                query = peerChatDb.Chanprops.Where(b => b.channelmask.Equals(lookup.channelmask));
            }else if(lookup.channelmask != null && lookup.channelmask.Contains("*")) { //wildcard match
            }
            return await query.ToListAsync();
        }

        public Task<ChanpropsRecord> Update(ChanpropsRecord model)
        {
            throw new NotImplementedException();
        }

        public async Task<ChanpropsRecord> GetEffectiveChanprops(string channel_name) {
            var query = peerChatDb.Chanprops as IQueryable<ChanpropsRecord>;
            var allChanProps = await query.ToListAsync();
            ChanpropsRecord longestMatch = null;
            int longestMatchLength = 0;
            foreach(var item in allChanProps) {
                if(FastWildcard.FastWildcard.IsMatch(channel_name, item.channelmask)) {
                    if(item.channelmask.Length > longestMatchLength) {
                        longestMatchLength = item.channelmask.Length;
                        longestMatch = item;
                    }
                }
            } 
            return longestMatch;
        }
        private int GetChannelId(string channel_name) {
            var db = peerChatCacheDb.GetDatabase();
            var key = db.StringGet("channelname_" + channel_name);
            if(int.TryParse(key.ToString(), out int channel_id)) {
                return channel_id;
            }
            return 0;
        }
        private async Task ApplyEffectiveChanprops(string channel_name, ChanpropsRecord record) {
            var db = peerChatCacheDb.GetDatabase();
            int channel_id = GetChannelId(channel_name);
            if(channel_id == 0) return;
            var key = "channel_" + channel_id;
            db.HashSet(key, "topic", record.topic);


            var Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan elapsedTime = (DateTime)record.setAt - Epoch;
            var setAt = ((int)elapsedTime.TotalSeconds);
            await db.HashSetAsync(key, "topic_time", setAt);
            await db.HashSetAsync(key, "topic_user", "SERVER");

            if(!string.IsNullOrEmpty(record.password)) {
                await db.HashSetAsync(key, "password", record.password);
            } else {
                await db.HashDeleteAsync(key, "password");
            }
            if(record.limit > 0) {
                await db.HashSetAsync(key, "limit", record.limit);
            } else {
                await db.HashDeleteAsync(key, "limit");
            }
            await db.HashSetAsync(key, "modeflags", record.modeflags);
            await db.HashSetAsync(key, "entrymsg", record.entrymsg);
            
        }
        public async Task<ChanpropsRecord> ApplyEffectiveChanprops(string channel_name) {
            var props = await GetEffectiveChanprops(channel_name);
            await ApplyEffectiveChanprops(channel_name, props);
            return props;
        }
    }
}