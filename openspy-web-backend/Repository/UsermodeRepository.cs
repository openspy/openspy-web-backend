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
    enum EUserChannelFlag
    {
        EUserChannelFlag_IsInChannel = 1 << 0,
        EUserChannelFlag_Voice = 1 << 1,
        EUserChannelFlag_HalfOp = 1 << 2,
        EUserChannelFlag_Op = 1 << 3,
        EUserChannelFlag_Owner = 1 << 4,
        EUserChannelFlag_Invisible = 1 << 5,
        EUserChannelFlag_Gagged = 1 << 6,
        EUserChannelFlag_Invited = 1 << 7,
        EUserChannelFlag_Quiet = 1 << 8,
        EUserChannelFlag_Banned = 1 << 9,
        EUserChannelFlag_GameidPermitted = 1 << 10,
    };

    public class UsermodeRepository : IRepository<UsermodeRecord, UsermodeLookup>
    {
        private PeerchatDBContext peerChatDb;
        private PeerchatCacheDatabase peerChatCacheDb;
        private IMQConnectionFactory connectionFactory;
        private String PEERCHAT_EXCHANGE;
        private String PEERCAHT_CLIENT_MESSAGE_KEY;
        private String PEERCHAT_KEYUPDATE_KEY;
        public UsermodeRepository(PeerchatDBContext peerChatDb, PeerchatCacheDatabase peerChatCacheDb, IMQConnectionFactory connectionFactory)
        {
            PEERCHAT_EXCHANGE = "peerchat.core";
            PEERCAHT_CLIENT_MESSAGE_KEY = "peerchat.client-messages";
            PEERCHAT_KEYUPDATE_KEY = "peerchat.keyupdate-messages";
            this.peerChatDb = peerChatDb;
            this.peerChatCacheDb = peerChatCacheDb;
            this.connectionFactory = connectionFactory;
        }
        private UsermodeRecord LookupTemporaryUsermode(IDatabase db, int usermodeId) {
            UsermodeRecord result = new UsermodeRecord();
            var key = "USERMODE_" + usermodeId;
            if(!db.KeyExists(key))
                return null;
            result.Id = usermodeId;

            RedisValue[] values = {
                "chanmask", "hostmask", "comment", "machineid", "modeflags", "setByNick", "setByHost", "setByPid", "profileid", "setAt", "expiresAt"
            };
            var redisValues = db.HashGet(key, values);
            
            result.channelmask = redisValues[0];
            result.hostmask = redisValues[1];
            result.comment = redisValues[2];
            result.machineid = redisValues[3];
            if(int.TryParse(redisValues[4], out int modeflags)) {
                result.modeflags = modeflags;
            } else {
                result.modeflags = 0;
            }
            
            result.isGlobal = usermodeId > 0;
            result.setByNick = redisValues[5];
            result.setByHost = redisValues[6];

            if(int.TryParse(redisValues[7], out int setByPid)) {
                result.setByPid = setByPid;
            }
            

            if(int.TryParse(redisValues[8], out int profileid)) {
                if(profileid == 0) 
                    result.profileid = null;
                else 
                    result.profileid = profileid;
            }
            
            var Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            result.setAt = Epoch.AddSeconds(int.Parse(redisValues[9]));

            result.expiresAt = Epoch.AddSeconds(int.Parse(redisValues[10]));
            return result;
        }
        private IEnumerable<UsermodeRecord> LookupCachedUsermodes(UsermodeLookup lookup) {
            var result = new List<UsermodeRecord>();
            var db = peerChatCacheDb.GetDatabase();
            IScanningCursor cursor = null;
            IEnumerable<SortedSetEntry> entries;
            do
            {
                entries = db.SortedSetScan("usermodes", cursor: cursor?.Cursor ?? 0);
                foreach (var entry in entries)
                {
                    int id = int.Parse(entry.Element.ToString());
                    if(id > 0) continue;
                    var usermode = LookupTemporaryUsermode(db, id);
                    if(usermode != null && TestUsermodeMatches(lookup, usermode)) {
                        result.Add(usermode);
                    } else if(usermode == null) {
                        db.SortedSetRemove("usermodes", entry.Element);
                    }
                }

                cursor = (IScanningCursor)entries;
            } while ((cursor?.Cursor ?? 0) != 0);
            return result;
        }
 
        private bool TestUsermodeMatches(UsermodeLookup lookup, UsermodeRecord usermode) {
            if(usermode.profileid.HasValue && lookup.profileid.HasValue) {
                if(usermode.profileid != lookup.profileid)
                    return false;
            }
            if(!string.IsNullOrEmpty(usermode.channelmask) && !string.IsNullOrEmpty(lookup.channelmask)) {
                return IRCMatch.match(usermode.channelmask, lookup.channelmask) == 0 || lookup.channelmask.Equals(usermode.channelmask);
            }
            if(!string.IsNullOrEmpty(usermode.hostmask) && !string.IsNullOrEmpty(lookup.hostmask)) {
                return IRCMatch.match(usermode.hostmask, lookup.hostmask) == 0;
            }
            if(!string.IsNullOrEmpty(usermode.machineid)) {
                return lookup.machineid.CompareTo(usermode.machineid) == 0;
            }
            return true;
        }
        public async Task<IEnumerable<UsermodeRecord>> Lookup(UsermodeLookup lookup)
        {
            var db = peerChatCacheDb.GetDatabase();
            List<UsermodeRecord> result, cachedResult = null;
            if((lookup.Id.HasValue && lookup.Id < 0)) {
                result = new List<UsermodeRecord>();
                var item = LookupTemporaryUsermode(db, lookup.Id.Value);
                if(item == null)
                    return result;
                result.Add(item);
                return result;
            } else if ((lookup.channelmask != null && lookup.channelmask.Contains("*") == false)) {
                //lookup cache
                cachedResult = new List<UsermodeRecord>(LookupCachedUsermodes(lookup));
            }

            
            var query = peerChatDb.Usermode as IQueryable<UsermodeRecord>;
            if (lookup.Id.HasValue)
            {
                query = peerChatDb.Usermode.Where(b => b.Id == lookup.Id.Value);
            }
            
            if (lookup.profileid.HasValue)
            {
                query = query.Where(b => b.profileid == lookup.profileid.Value || !b.profileid.HasValue);
            }
            if (lookup.machineid != null)
            {
                query = query.Where(b => b.machineid == lookup.machineid || b.machineid == null);
            }

            if (lookup.gameid.HasValue)
            {
                query = query.Where(b => b.gameid == lookup.gameid || !b.gameid.HasValue);
            }

            query = query.Where(b => b.expiresAt == null || b.expiresAt > DateTime.UtcNow);
            result = await query.ToListAsync();

            List<UsermodeRecord> filteredResult = cachedResult ?? new List<UsermodeRecord>();
            //perform wildcard reduction... needs to be client side for now :(
            foreach(var item in result) {
                bool hostMatch = lookup.hostmask == null || item.hostmask == null || IRCMatch.match(item.hostmask, lookup.hostmask) == 0;
                bool chanMatch = lookup.channelmask == null || item.channelmask == null || IRCMatch.match(item.channelmask, lookup.channelmask) == 0;
                if(hostMatch && chanMatch) {
                    filteredResult.Add(item);
                }
            }
            

            if(lookup.expired.HasValue) {
                filteredResult = PerformExpiredFiltering(filteredResult, lookup.expired.Value);
            }
            return filteredResult;
        }
        private List<UsermodeRecord> PerformExpiredFiltering(List<UsermodeRecord> input, bool expired)  {
            List<UsermodeRecord> result = new List<UsermodeRecord>();
            
            foreach(var item in input) {
                bool isExpired = item.expiresAt.HasValue && item.expiresAt <= item.setAt;
                if(expired == isExpired) {
                    result.Add(item);
                }
            }
            return result;
        }
        public Task<bool> Delete(UsermodeLookup lookup)
        {
            return Task.Run(async () =>
            {
                var total_modified = 0;
                var usermodes = (await Lookup(lookup)).ToList();
                foreach (var usermode in usermodes)
                {
                    if(usermode.Id < 0) {
                        total_modified++;
                        DeleteTemporaryUsermode(usermode.Id);
                    } else {
                        peerChatDb.Remove<UsermodeRecord>(usermode);
                        total_modified += await peerChatDb.SaveChangesAsync();
                    }
                    
                    await ResyncChannelUsermodes(usermode.channelmask);
                }

                SendBansUpdate(usermodes, false);
                return usermodes.Count > 0 && total_modified > 0;
            });
        }
        private void DeleteTemporaryUsermode(int id) {
            var db = peerChatCacheDb.GetDatabase();
            db.KeyDelete("USERMODE_" + id.ToString());
        }
        private void SendBansUpdate(IEnumerable<UsermodeRecord> usermodes, bool add)
        {
            var db = peerChatCacheDb.GetDatabase();
            IScanningCursor cursor = null;
            IEnumerable<HashEntry> entries;

            foreach (var usermode in usermodes)
            {
                if (usermode.hostmask != null && usermode.hostmask.Length > 0 && (usermode.modeflags & (int)EUserChannelFlag.EUserChannelFlag_Banned) != 0)
                {
                    do
                    {
                        entries = db.HashScan("channels", usermode.channelmask, 10, cursor?.Cursor ?? 0);
                        foreach (var entry in entries)
                        {
                            ConnectionFactory factory = connectionFactory.Get();
                            using (IConnection connection = factory.CreateConnection())
                            {
                                using (IModel channel = connection.CreateModel())
                                {
                                    var modeString = (add ? "+" : "-") + "b *@" + usermode.hostmask;
                                    SendModeString(entry.Value, modeString);
                                }
                            }
                        }
                        cursor = (IScanningCursor)entries;
                    } while ((cursor?.Cursor ?? 0) != 0);
                }
            }
        }
        private void SendModeString(String toChannel, String modeString) {
            ConnectionFactory factory = connectionFactory.Get();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    var modeStringBytes = System.Text.Encoding.UTF8.GetBytes(modeString);
                    String message = String.Format("\\type\\MODE\\toChannelId\\{0}\\message\\{1}\\fromUserId\\-1\\includeSelf\\1", toChannel, Convert.ToBase64String(modeStringBytes));
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/plain";
                    channel.BasicPublish(PEERCHAT_EXCHANGE, PEERCAHT_CLIENT_MESSAGE_KEY, props, messageBodyBytes);
                }
            }
        }
        public async Task<UsermodeRecord> Update(UsermodeRecord model)
        {
            var entry = peerChatDb.Update<UsermodeRecord>(model);
            await peerChatDb.SaveChangesAsync();
            return entry.Entity;
        }
        public async Task<UsermodeRecord> Create(UsermodeRecord model)
        {
            if(model.isGlobal == true) {
                model.setAt = DateTime.UtcNow;
                var entry = await peerChatDb.AddAsync<UsermodeRecord>(model);
                var num_modified = await peerChatDb.SaveChangesAsync();
                await ApplyUserrecords(entry.Entity);
                SendBansUpdate(new List<UsermodeRecord> { entry.Entity }, false);
                await ResyncChannelUsermodes(model.channelmask);
                
                return entry.Entity;
            } else {
                if(model.channelmask.Contains("*")) return null;
                model.Id = WriteTemporaryUsermode(model);
                SendBansUpdate(new List<UsermodeRecord> { model }, true);
                await ResyncChannelUsermodes(model.channelmask);
                return model;
            }            
        }
        private int WriteTemporaryUsermode(UsermodeRecord model) {
            var db = peerChatCacheDb.GetDatabase();
            var id = -(db.StringIncrement("USERMODEID"));

            var redis_Key = "USERMODE_" + id;

            db.HashSet(redis_Key, "id", id);
            db.HashSet(redis_Key, "chanmask", model.channelmask);
            db.HashSet(redis_Key, "hostmask", model.hostmask);
            db.HashSet(redis_Key, "comment", model.comment);
            db.HashSet(redis_Key, "machineid", model.machineid);
            db.HashSet(redis_Key, "modeflags", model.modeflags);
            db.HashSet(redis_Key, "isGlobal", model.isGlobal ? 1 : 0);
            db.HashSet(redis_Key, "setByNick", model.setByNick);
            db.HashSet(redis_Key, "setByHost", model.setByHost);
            db.HashSet(redis_Key, "setByPid", model.setByPid);

            var Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan elapsedTime = (DateTime)model.setAt - Epoch;
            db.HashSet(redis_Key, "setAt", (int)elapsedTime.TotalSeconds);

            if(model.expiresAt != null) {
                elapsedTime = (DateTime)model.expiresAt - Epoch;
                db.HashSet(redis_Key, "expiresAt", (int)elapsedTime.TotalSeconds);   
            } else {
                db.HashSet(redis_Key, "expiresAt", 0);
            }

            db.SortedSetIncrement("usermodes", id, 1);
            return (int)id;
        }
        private async Task ApplyUserrecords(UsermodeRecord model)
        {
            var db = peerChatCacheDb.GetDatabase();
            IScanningCursor cursor = null;
            IEnumerable<HashEntry> entries;
            do
            {
                entries = db.HashScan("channels", model.channelmask, 10, cursor?.Cursor ?? 0);
                foreach (var entry in entries)
                {
                    await ApplyUsermodesToChannel(entry.Name, model);
                }
                cursor = (IScanningCursor)entries;
            } while ((cursor?.Cursor ?? 0) != 0);
        }
        /// <summary>
        /// This should be MOVED!
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        PeerchatUserSummary GetSummaryByUserid(int userid)
        {
            PeerchatUserSummary summary = new PeerchatUserSummary();
            var user_key = "user_" + userid;
            var db = peerChatCacheDb.GetDatabase();
            summary.Nick = db.HashGet(user_key, "nick");
            summary.Username = db.HashGet(user_key, "username");
            summary.Realname = db.HashGet(user_key, "realname");
            summary.Hostname = db.HashGet(user_key, "hostname");
            summary.Profileid = int.Parse(db.HashGet(user_key, "profileid"));
            summary.Id = userid;
            return summary;
        }
        private async Task ApplyUsermodesToChannel(string channelName, UsermodeRecord model)
        {
            var db = peerChatCacheDb.GetDatabase();
            var id = db.StringGet("channel_" + channelName);
            //scan channel users
            var channel_users_key = "channel_" + id + "_users";
            IScanningCursor cursor = null;
            IEnumerable<SortedSetEntry> entries;
            do
            {
                entries = db.SortedSetScan(channel_users_key, cursor: cursor?.Cursor ?? 0);
                foreach (var entry in entries)
                {
                    var userId = int.Parse(entry.Element);
                    var summary = GetSummaryByUserid(userId);
                    var channelUserSummary = new PeerchatChannelUserSummary();
                    channelUserSummary.ChannelName = channelName;
                    channelUserSummary.UserSummary = summary;
                    var effectiveUsermode = await GetEffectiveUsermode(channelUserSummary);
                    await ApplyUsermode(channelUserSummary, effectiveUsermode);
                }
            } while ((cursor?.Cursor ?? 0) != 0);

            //
            if(!string.IsNullOrEmpty(model.hostmask) && model.hostmask.Length > 0 && (model.modeflags & (int)EUserChannelFlag.EUserChannelFlag_Banned) != 0)
            {
                ConnectionFactory factory = connectionFactory.Get();
                using (IConnection connection = factory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        var modeString = "+b *@" + model.hostmask;
                        var modeStringBytes = System.Text.Encoding.UTF8.GetBytes(modeString);
                        String message = String.Format("\\type\\MODE\\toChannelId\\{0}\\message\\{1}\\fromUserId\\-1\\includeSelf\\1", id, Convert.ToBase64String(modeStringBytes));
                        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                        IBasicProperties props = channel.CreateBasicProperties();
                        props.ContentType = "text/plain";
                        channel.BasicPublish(PEERCHAT_EXCHANGE, PEERCAHT_CLIENT_MESSAGE_KEY, props, messageBodyBytes);
                    }
                }
            }

        }
        private bool checkFlagRemoved(EUserChannelFlag flag, int newFlags, int oldFlags)
        {
            return ((newFlags & (int)flag) == 0) && ((oldFlags & (int)flag) != 0);
        }
        private bool checkFlagAdded(EUserChannelFlag flag, int newFlags, int oldFlags)
        {
            return ((oldFlags & (int)flag) == 0) && ((newFlags & (int)flag) != 0);
        }
        public async Task ApplyUsermode(PeerchatChannelUserSummary channelUserSummary, UsermodeRecord usermode)
        {
            bool kickUser = false;
            if(usermode == null)
            {
                usermode = await GetEffectiveUsermode(channelUserSummary);
            }
            var db = peerChatCacheDb.GetDatabase();
            var channelId = db.StringGet("channelname_" + channelUserSummary.ChannelName);
            var channel_key = "channel_" + channelId.ToString() + "_user_" + channelUserSummary.UserSummary.Id;

            var redisValue = db.HashGet(channel_key, "modeflags");
            int currentModeflags = int.Parse(redisValue.ToString());

            int modeflags = usermode.modeflags.HasValue ? usermode.modeflags.Value : 0;
            int newModeflags = modeflags | (int)EUserChannelFlag.EUserChannelFlag_IsInChannel;
            string removeString = "";
            string addString = "";

            string userString = "";

            if (checkFlagRemoved(EUserChannelFlag.EUserChannelFlag_Voice, newModeflags, currentModeflags))
            {
                removeString += "v";
                userString += " " + channelUserSummary.UserSummary.Nick;
            }
            if (checkFlagRemoved(EUserChannelFlag.EUserChannelFlag_HalfOp, newModeflags, currentModeflags) || checkFlagRemoved(EUserChannelFlag.EUserChannelFlag_Op, newModeflags, currentModeflags) || checkFlagRemoved(EUserChannelFlag.EUserChannelFlag_Owner, newModeflags, currentModeflags))
            {
                removeString += "o";
                userString += " " + channelUserSummary.UserSummary.Nick;
            }

            if (checkFlagAdded(EUserChannelFlag.EUserChannelFlag_Voice, newModeflags, currentModeflags))
            {
                addString += "v";
                userString += " " + channelUserSummary.UserSummary.Nick;
            }

            //remove banned user
            if (checkFlagAdded(EUserChannelFlag.EUserChannelFlag_Banned, newModeflags, currentModeflags))
            {
                kickUser = true;   
            }
            if (checkFlagAdded(EUserChannelFlag.EUserChannelFlag_HalfOp, newModeflags, currentModeflags) || checkFlagAdded(EUserChannelFlag.EUserChannelFlag_Op, newModeflags, currentModeflags) || checkFlagAdded(EUserChannelFlag.EUserChannelFlag_Owner, newModeflags, currentModeflags))
            {
                addString += "o";
                userString += " " + channelUserSummary.UserSummary.Nick;
            }

            db.HashSet(channel_key, "modeflags", newModeflags);

            var modeString = "";
            if (removeString.Length > 0)
            {
                modeString += "-" + removeString;
            }
            if (addString.Length > 0)
            {
                modeString += "+" + addString;
            }
            modeString += userString;


            if (modeString.Length == 0 && !kickUser) return;

            ConnectionFactory factory = connectionFactory.Get();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/plain";
                    if(modeString.Length > 0) {
                        var modeStringBytes = System.Text.Encoding.UTF8.GetBytes(modeString);
                        String message = String.Format("\\type\\MODE\\toChannelId\\{0}\\message\\{1}\\fromUserId\\-1\\includeSelf\\1", channelId.ToString(), Convert.ToBase64String(modeStringBytes));
                        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message); 
                        channel.BasicPublish(PEERCHAT_EXCHANGE, PEERCAHT_CLIENT_MESSAGE_KEY, props, messageBodyBytes);                        
                    }


                    if(kickUser) {
                        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes("Banned");
                        String kickMessage = String.Format("\\type\\KICK\\toChannelId\\{0}\\message\\{1}\\fromUserId\\-1\\includeSelf\\1\\toUserSummary\\{2}", channelId.ToString(), Convert.ToBase64String(nameBytes), channelUserSummary.UserSummary.ToString());
                        byte[] kickMessageBytes = System.Text.Encoding.UTF8.GetBytes(kickMessage);
                        channel.BasicPublish(PEERCHAT_EXCHANGE, PEERCAHT_CLIENT_MESSAGE_KEY, props, kickMessageBytes);
                        newModeflags = 0;
                        db.SortedSetRemove("channel_" + channelId + "_users", channelUserSummary.UserSummary.Id);
                        db.KeyDelete("channel_" + channelId + "_user_" + channelUserSummary.UserSummary.Id);

                    }

                    String modeflagsMessage = string.Format("\\type\\UPDATE_USER_CHANMODEFLAGS\\to\\{0}\\user_id\\{1}\\modeflags\\{2}", channelUserSummary.ChannelName, channelUserSummary.UserSummary.Id, newModeflags);
                    byte[] modeflagsMessageBytes = System.Text.Encoding.UTF8.GetBytes(modeflagsMessage);
                    channel.BasicPublish(PEERCHAT_EXCHANGE, PEERCHAT_KEYUPDATE_KEY, props, modeflagsMessageBytes);
                }
            }
        }
        public async Task<UsermodeRecord> GetEffectiveUsermode(PeerchatChannelUserSummary channelUserSummary)
        {
            var lookup = new UsermodeLookup();
            lookup.channelmask = channelUserSummary.ChannelName;
            lookup.hostmask = channelUserSummary.UserSummary.Hostname;
            lookup.machineid = channelUserSummary.UserSummary.Realname;
            lookup.profileid = channelUserSummary.UserSummary.Profileid;
            lookup.gameid = channelUserSummary.UserSummary.Gameid;

            var record = new UsermodeRecord();
            record.channelmask = channelUserSummary.ChannelName;
            record.expiresAt = DateTime.MaxValue;

            var usermodes = (await Lookup(lookup)).ToList();
            foreach(var usermode in usermodes)
            {
                record.modeflags |= usermode.modeflags;
                if(usermode.expiresAt.HasValue && usermode.expiresAt < record.expiresAt)
                {
                    record.expiresAt = usermode.expiresAt;
                }
            }

            if(record.expiresAt == DateTime.MaxValue)
            {
                record.expiresAt = null;
            }

            return record;
        }
        private IEnumerable<int> GetChannelUsers(int channelId)
        {
            var db = peerChatCacheDb.GetDatabase();
            var redis_key = "channel_" + channelId + "_users";
            IScanningCursor cursor = null;
            IEnumerable<SortedSetEntry> entries;
            List<int> result = new List<int>();
            do
            {
                entries = db.SortedSetScan(redis_key, cursor: cursor?.Cursor ?? 0);
                foreach (var entry in entries)
                {
                    result.Add(int.Parse(entry.Element));
                    

                }
                cursor = (IScanningCursor)entries;
            } while ((cursor?.Cursor ?? 0) != 0);
            return result;
        }

        public async Task ResyncChannelUsermodes(string channelMask)
        {
            var db = peerChatCacheDb.GetDatabase();
            IScanningCursor cursor = null;
            IEnumerable<HashEntry> entries;
            do
            {
                entries = db.HashScan("channels", channelMask, 10, cursor?.Cursor ?? 0);
                foreach(var entry in entries)
                {
                    var channelId = int.Parse(entry.Value);
                    var userIds = GetChannelUsers(channelId);
                    foreach (var userId in userIds)
                    {
                        var summary = GetSummaryByUserid(userId);
                        var chanUserSummary = new PeerchatChannelUserSummary();
                        chanUserSummary.ChannelName = entry.Name;
                        chanUserSummary.UserSummary = summary;
                        await ApplyUsermode(chanUserSummary, null);
                    }
                }
                cursor = (IScanningCursor)entries;
            } while ((cursor?.Cursor ?? 0) != 0);
        }
    }
}