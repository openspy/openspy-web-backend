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
    };

    public class UsermodeRepository : IRepository<UsermodeRecord, UsermodeLookup>
    {
        //PEERCHAT_EXCHANGE, PEERCAHT_CLIENT_MESSAGE_KEY
        private PeerchatDBContext peerChatDb;
        private PeerchatCacheDatabase peerChatCacheDb;
        private IMQConnectionFactory connectionFactory;
        private String PEERCHAT_EXCHANGE;
        private String PEERCAHT_CLIENT_MESSAGE_KEY;
        public UsermodeRepository(PeerchatDBContext peerChatDb, PeerchatCacheDatabase peerChatCacheDb, IMQConnectionFactory connectionFactory)
        {
            PEERCHAT_EXCHANGE = "openspy.core";
            PEERCAHT_CLIENT_MESSAGE_KEY = "peerchat.client-messages";
            this.peerChatDb = peerChatDb;
            this.peerChatCacheDb = peerChatCacheDb;
            this.connectionFactory = connectionFactory;
        }
        public async Task<IEnumerable<UsermodeRecord>> Lookup(UsermodeLookup lookup)
        {
            var query = peerChatDb.Usermode as IQueryable<UsermodeRecord>;
            if (lookup.Id.HasValue)
            {
                query = peerChatDb.Usermode.Where(b => b.Id == lookup.Id.Value);
            }
            
            if (lookup.channelmask != null && lookup.channelmask.CompareTo("*") == 0) { //chan mask set... and global wildcard
                if(lookup.channelmask.Contains("*")) //wildcard search
                {
                    var mask = lookup.channelmask.Replace("*", "%");
                    query = query.Where(b => EF.Functions.Like(b.channelmask, mask));
                } else
                {
                    query = query.Where(b => b.channelmask == lookup.channelmask);
                }
            }
            if (lookup.profileid.HasValue)
            {
                query = query.Where(b => b.profileid == lookup.profileid.Value || !b.profileid.HasValue);
            }
            if (lookup.machineid != null)
            {
                query = query.Where(b => b.machineid == lookup.machineid || b.machineid == null);
            }
            if (lookup.hostmask != null)
            {
                if (lookup.channelmask.Contains("*")) //wildcard search
                {
                    var mask = lookup.hostmask.Replace("*", "%");
                    query = query.Where(b => EF.Functions.Like(b.hostmask, mask) || b.hostmask == null);
                } else
                {
                    query = query.Where(b => b.hostmask == lookup.hostmask || b.hostmask == null);
                }
            }
            query = query.Where(b => b.expiresAt == null || b.expiresAt > DateTime.UtcNow);
            return await query.ToListAsync();
        }
        public Task<bool> Delete(UsermodeLookup lookup)
        {
            return Task.Run(async () =>
            {
                var total_modified = 0;
                var usermodes = (await Lookup(lookup)).ToList();
                foreach (var usermode in usermodes)
                {
                    peerChatDb.Remove<UsermodeRecord>(usermode);
                    total_modified += await peerChatDb.SaveChangesAsync();
                    await ResyncChannelUsermodes(usermode.channelmask);
                }

                SendBansUpdate(usermodes, false);
                return usermodes.Count > 0 && total_modified > 0;
            });
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
                                    var modeStringBytes = System.Text.Encoding.UTF8.GetBytes(modeString);
                                    String message = String.Format("\\type\\MODE\\toChannelId\\{0}\\message\\{1}\\fromUserId\\-1\\includeSelf\\1", entry.Value, Convert.ToBase64String(modeStringBytes));
                                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                                    IBasicProperties props = channel.CreateBasicProperties();
                                    props.ContentType = "text/plain";
                                    channel.BasicPublish(PEERCHAT_EXCHANGE, PEERCAHT_CLIENT_MESSAGE_KEY, props, messageBodyBytes);
                                }
                            }
                        }
                        cursor = (IScanningCursor)entries;
                    } while ((cursor?.Cursor ?? 0) != 0);
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
            model.setAt = DateTime.UtcNow;
            var entry = await peerChatDb.AddAsync<UsermodeRecord>(model);
            var num_modified = await peerChatDb.SaveChangesAsync();
            await ApplyUserrecords(entry.Entity);
            SendBansUpdate(new List<UsermodeRecord> { entry.Entity }, false);
            return entry.Entity;
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
            if(model.hostmask.Length > 0 && (model.modeflags & (int)EUserChannelFlag.EUserChannelFlag_Banned) != 0)
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
            if(usermode == null)
            {
                usermode = await GetEffectiveUsermode(channelUserSummary);
            }
            var db = peerChatCacheDb.GetDatabase();
            var channelId = db.StringGet("channelname_" + channelUserSummary.ChannelName);
            var channel_key = "channel_" + channelId.ToString() + "_user_" + channelUserSummary.UserSummary.Id;

            var redisValue = db.HashGet(channel_key, "modeflags");
            int currentModeflags = int.Parse(redisValue.ToString());

            int newModeflags = usermode.modeflags | (int)EUserChannelFlag.EUserChannelFlag_IsInChannel;
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


            if (modeString.Length == 0) return;

            ConnectionFactory factory = connectionFactory.Get();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {

                    var modeStringBytes = System.Text.Encoding.UTF8.GetBytes(modeString);
                    String message = String.Format("\\type\\MODE\\toChannelId\\{0}\\message\\{1}\\fromUserId\\-1\\includeSelf\\1", channelId.ToString(), Convert.ToBase64String(modeStringBytes));
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/plain";
                    channel.BasicPublish(PEERCHAT_EXCHANGE, PEERCAHT_CLIENT_MESSAGE_KEY, props, messageBodyBytes);
                }
            }
        }
        public async Task<UsermodeRecord> GetEffectiveUsermode(PeerchatChannelUserSummary channelUserSummary)
        {
            var lookup = new UsermodeLookup();
            lookup.channelmask = channelUserSummary.ChannelName;
            lookup.hostmask = channelUserSummary.UserSummary.Hostname;
            lookup.profileid = channelUserSummary.UserSummary.Profileid;

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
