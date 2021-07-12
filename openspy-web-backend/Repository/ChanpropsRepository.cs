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

        private String PEERCHAT_EXCHANGE;
        private String PEERCAHT_CLIENT_MESSAGE_KEY;
        private String PEERCHAT_KEYUPDATE_KEY;
        enum EChannelBasicModes {
            EChannelMode_NoOutsideMessages = 1 << 0, //+n
            EChannelMode_TopicProtect = 1 << 1, // +t
            EChannelMode_Moderated = 1 << 2, // +m
            EChannelMode_Private = 1 << 3, // +p
            EChannelMode_Secret = 1 << 4, // +s
            EChannelMode_InviteOnly = 1 << 5, // +i
            EChannelMode_StayOpen = 1 << 6, //+z??
            EChannelMode_Registered = 1 << 7, //+r 
            EChannelMode_OpsObeyChannelLimit = 1 << 8, //+e -- maybe "ops obey channel limit"?
            EChannelMode_Auditorium = 1 << 9, //+u
            EChannelMode_Auditorium_ShowVOP = 1 << 10, //+q
            //
        };
        public ChanpropsRepository(PeerchatDBContext peerChatDb, PeerchatCacheDatabase peerChatCacheDb, IMQConnectionFactory connectionFactory)
        {
            PEERCHAT_EXCHANGE = "openspy.core";
            PEERCAHT_CLIENT_MESSAGE_KEY = "peerchat.client-messages";
            PEERCHAT_KEYUPDATE_KEY = "peerchat.keyupdate-messages";
            this.peerChatDb = peerChatDb;
            this.peerChatCacheDb = peerChatCacheDb;
            this.connectionFactory = connectionFactory;
        }
        public async Task<ChanpropsRecord> Create(ChanpropsRecord model)
        {

            var tracking = peerChatDb.ChangeTracker.QueryTrackingBehavior;
            peerChatDb.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            
            var existingRecord = await Lookup(new ChanpropsLookup { channelmask = model.channelmask});
            var record = existingRecord.FirstOrDefault();
            model.setAt = DateTime.UtcNow;
            if(record == null) {                
                var entry = await peerChatDb.AddAsync<ChanpropsRecord>(model);
                var num_modified = await peerChatDb.SaveChangesAsync();
                peerChatDb.ChangeTracker.QueryTrackingBehavior = tracking;
                await ResyncChanPropsForChanMask(model.channelmask, model.kickExisting ?? false);
                return entry.Entity;
            } else {
                model.Id = record.Id;
                var entry = peerChatDb.Update<ChanpropsRecord>(model);
                await peerChatDb.SaveChangesAsync();
                peerChatDb.ChangeTracker.QueryTrackingBehavior = tracking;
                await ResyncChanPropsForChanMask(model.channelmask, model.kickExisting ?? false);

                
                return entry.Entity;
            }
        }

        public async Task<bool> Delete(ChanpropsLookup lookup)
        {
            var existingRecord = await Lookup(lookup);
            var record = existingRecord.FirstOrDefault();
            if(record != null) {
                peerChatDb.Chanprops.Remove(record);
                return (await peerChatDb.SaveChangesAsync()) > 0;
            }
            return false;
        }

        private IEnumerable<ChanpropsRecord> PerformExistsFiltering(IEnumerable<ChanpropsRecord> input, bool exists)  {
            IEnumerable<ChanpropsRecord> result = new List<ChanpropsRecord>();
            var db = peerChatCacheDb.GetDatabase();
            foreach(var item in input) {
                var key = "channelname_" + item.channelmask;
                var keyExists = db.KeyExists(key);
                if(keyExists == exists) {
                    result = result.Append(item);
                }                
            }
            return result;
        }


        private IEnumerable<ChanpropsRecord> PerformExpiredFiltering(IEnumerable<ChanpropsRecord> input, bool expired)  {
            IEnumerable<ChanpropsRecord> result = new List<ChanpropsRecord>();
            
            foreach(var item in input) {
                bool isExpired = item.expiresAt.HasValue && item.expiresAt <= item.setAt;
                if(expired == isExpired) {
                    result = result.Append(item);
                }
            }
            return result;
        }

        public async Task<IEnumerable<ChanpropsRecord>> Lookup(ChanpropsLookup lookup)
        {
            var query = peerChatDb.Chanprops as IQueryable<ChanpropsRecord>;
            if (lookup.Id.HasValue)
            {
                query = peerChatDb.Chanprops.Where(b => b.Id == lookup.Id.Value);
            } else if(lookup.channelmask != null) {
                query = peerChatDb.Chanprops.Where(b => b.channelmask.Equals(lookup.channelmask));
            } else if(lookup.modeflagMask.HasValue) {
                query = peerChatDb.Chanprops.Where(b => (b.modeflags & lookup.modeflagMask.Value) != 0);
            }

            IEnumerable<ChanpropsRecord> result = await query.ToListAsync();
            if(lookup.exists.HasValue) {
                result = PerformExistsFiltering(result, lookup.exists.Value);
            }

            if(lookup.expired.HasValue) {
                result = PerformExpiredFiltering(result, lookup.expired.Value);
            }
            return result;
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
                if(IRCMatch.match(item.channelmask, channel_name) == 0) {
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
        private void SendUpdatePassword(int channel_id, string current_password, string password) {
            if(string.IsNullOrEmpty(current_password)) {
                current_password = null;
            }

            if(string.IsNullOrEmpty(password)) {
                password = null;
            }

            ConnectionFactory factory = connectionFactory.Get();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    string modeString = "";

                    if(password == null && current_password != null) {
                        modeString = "-k";
                    }
                    else if(password != null && !password.Equals(current_password)) {
                        modeString = "+k " + password;
                    }

                    var modeStringBytes = System.Text.Encoding.UTF8.GetBytes(modeString);
                    String message = String.Format("\\type\\MODE\\toChannelId\\{0}\\message\\{1}\\fromUserId\\-1\\includeSelf\\1", channel_id, Convert.ToBase64String(modeStringBytes));
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/plain";
                    channel.BasicPublish(PEERCHAT_EXCHANGE, PEERCAHT_CLIENT_MESSAGE_KEY, props, messageBodyBytes);
                }
            }
        }
        private bool checkFlagRemoved(EChannelBasicModes flag, int newFlags, int oldFlags)
        {
            return ((newFlags & (int)flag) == 0) && ((oldFlags & (int)flag) != 0);
        }
        private bool checkFlagAdded(EChannelBasicModes flag, int newFlags, int oldFlags)
        {
            return ((oldFlags & (int)flag) == 0) && ((newFlags & (int)flag) != 0);
        }
        private void SendUpdateBasicModes(int channel_id, int current_modeflags, int modeflags) {
            Dictionary<EChannelBasicModes, string> modeMap = new Dictionary<EChannelBasicModes, string>();
            modeMap[EChannelBasicModes.EChannelMode_NoOutsideMessages] = "n";
            modeMap[EChannelBasicModes.EChannelMode_TopicProtect] = "t";
            modeMap[EChannelBasicModes.EChannelMode_Moderated] = "m";
            modeMap[EChannelBasicModes.EChannelMode_Private] = "p";
            modeMap[EChannelBasicModes.EChannelMode_Secret] = "s";
            modeMap[EChannelBasicModes.EChannelMode_InviteOnly] = "i";
            modeMap[EChannelBasicModes.EChannelMode_StayOpen] = "z";
            modeMap[EChannelBasicModes.EChannelMode_Registered] = "r";
            modeMap[EChannelBasicModes.EChannelMode_OpsObeyChannelLimit] = "e";
            modeMap[EChannelBasicModes.EChannelMode_Auditorium] = "u";
            modeMap[EChannelBasicModes.EChannelMode_Auditorium_ShowVOP] = "q";
            string addedString = "+", removedString = "-";
            foreach(var item in modeMap) {

                if(checkFlagRemoved(item.Key, modeflags, current_modeflags)) {
                    removedString += item.Value;
                }
                if(checkFlagAdded(item.Key, modeflags, current_modeflags)) {
                    addedString += item.Value;
                }
            }
            string modeString = "";
            if(removedString.Length > 1) {
                modeString += removedString;
            }
            if(addedString.Length > 1) {
                modeString += addedString;
            }

            if(string.IsNullOrEmpty(modeString)) {
                return;
            }

            ConnectionFactory factory = connectionFactory.Get();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    var modeStringBytes = System.Text.Encoding.UTF8.GetBytes(modeString);
                    String message = String.Format("\\type\\MODE\\toChannelId\\{0}\\message\\{1}\\fromUserId\\-1\\includeSelf\\1", channel_id, Convert.ToBase64String(modeStringBytes));
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/plain";
                    channel.BasicPublish(PEERCHAT_EXCHANGE, PEERCAHT_CLIENT_MESSAGE_KEY, props, messageBodyBytes);
                }
            }
        }
        private void SendUpdateLimit(int channel_id, int current_limit, int limit) {
            if(current_limit != limit) {
                ConnectionFactory factory = connectionFactory.Get();
                using (IConnection connection = factory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        string modeString = "";

                        if(limit == 0) {
                            modeString = "-l";
                        }
                        else if(limit > 0) {
                            modeString = "+l " + limit.ToString();
                        }

                        var modeStringBytes = System.Text.Encoding.UTF8.GetBytes(modeString);
                        String message = String.Format("\\type\\MODE\\toChannelId\\{0}\\message\\{1}\\fromUserId\\-1\\includeSelf\\1", channel_id, Convert.ToBase64String(modeStringBytes));
                        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                        IBasicProperties props = channel.CreateBasicProperties();
                        props.ContentType = "text/plain";
                        channel.BasicPublish(PEERCHAT_EXCHANGE, PEERCAHT_CLIENT_MESSAGE_KEY, props, messageBodyBytes);
                    }
                }
            }
        }
        private void SendUpdateTopic(int channel_id, ChanpropsRecord record) {
            ConnectionFactory factory = connectionFactory.Get();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    var modeStringBytes = System.Text.Encoding.UTF8.GetBytes(record.topic);
                    String message = String.Format("\\type\\TOPIC\\toChannelId\\{0}\\message\\{1}\\fromUserSummary\\SERVER!SERVER@0.0.0.0\\includeSelf\\1", channel_id, Convert.ToBase64String(modeStringBytes));
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/plain";
                    channel.BasicPublish(PEERCHAT_EXCHANGE, PEERCAHT_CLIENT_MESSAGE_KEY, props, messageBodyBytes);
                }
            }
        }
        public async Task<ChanpropsRecord> ApplyEffectiveChanprops(string channel_name, bool kickExisting = false) {
            var props = await GetEffectiveChanprops(channel_name);
            if(props == null && kickExisting) {
                props = new ChanpropsRecord{};
                props.kickExisting = true;
            } else if(props == null) {
                return null;
            } else {
                props.kickExisting = kickExisting;
            }
            
            await ApplyEffectiveChanprops(channel_name, props);
            return props;
        }
        private async Task ApplyEffectiveChanprops(string channel_name, ChanpropsRecord record) {
            var db = peerChatCacheDb.GetDatabase();
            int channel_id = GetChannelId(channel_name);
            if(channel_id == 0) return;
            var key = "channel_" + channel_id;

            if(record.kickExisting.HasValue && record.kickExisting.Value == true) {
                DeleteChannel(channel_id);
                return;
            }

            var current_topic = db.HashGet(key, "topic");
            if(string.IsNullOrEmpty(record.topic)) {
                    db.HashDelete(key, "topic");
                    db.HashDelete(key, "topic_user");
                    db.HashDelete(key, "topic_time");
            }
            else if(!current_topic.Equals(record.topic)) {
                db.HashSet(key, "topic", record.topic);

                var Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                TimeSpan elapsedTime = (DateTime)record.setAt - Epoch;
                var setAt = ((int)elapsedTime.TotalSeconds);
                await db.HashSetAsync(key, "topic_time", setAt);
                await db.HashSetAsync(key, "topic_user", "SERVER");
                
                SendUpdateTopic(channel_id, record);
            }



            
            var current_password = db.HashGet(key, "password");
            if(!string.IsNullOrEmpty(record.password)) {
                await db.HashSetAsync(key, "password", record.password);
            } else {
                await db.HashDeleteAsync(key, "password");
            }
            SendUpdatePassword(channel_id, current_password, record.password);

            var current_limit_string = db.HashGet(key, "limit");
            int current_limit = 0;
            if(!string.IsNullOrEmpty(current_limit_string)) {
                current_limit = int.Parse(current_limit_string);
            }

            if(record.limit > 0) {
                await db.HashSetAsync(key, "limit", record.limit);
            } else {
                await db.HashDeleteAsync(key, "limit");
            }
            SendUpdateLimit(channel_id, current_limit, record.limit ?? 0);


            var current_modeflags_string = db.HashGet(key, "modeflags");
            int current_modeflags = int.Parse(current_modeflags_string);
            await db.HashSetAsync(key, "modeflags", record.modeflags);
            SendUpdateBasicModes(channel_id, current_modeflags, record.modeflags);
            await db.HashSetAsync(key, "entrymsg", record.entrymsg);            

            if(!string.IsNullOrEmpty(record.groupname)) {
                await db.HashSetAsync(key, "custkey_groupname", record.groupname);
            }
        }
        private async Task ResyncChanPropsForChanMask(string channel_mask, bool kickExisting) {
            var db = peerChatCacheDb.GetDatabase();
            

            IScanningCursor cursor = null;
            IEnumerable<HashEntry> entries;
            do
            {
                entries = db.HashScan("channels", channel_mask, cursor: cursor?.Cursor ?? 0);
                foreach (var entry in entries)
                {
                    var name = db.HashGet("channel_" + entry.Value, "name");
                    await ApplyEffectiveChanprops(name, kickExisting);
                }

                cursor = (IScanningCursor)entries;
            } while ((cursor?.Cursor ?? 0) != 0);
        }
        private void DeleteChannel(int channel_id) {
            var db = peerChatCacheDb.GetDatabase();
            var channel_prefix = "channel_" + channel_id;
            var channel_name = db.StringGet("channelname_" + channel_id);
            db.HashSet(channel_prefix, "modeflags", (int)EChannelBasicModes.EChannelMode_InviteOnly); //set invite flag, so no one can join
            ConnectionFactory factory = connectionFactory.Get();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {


                    IScanningCursor cursor = null;
                    IEnumerable<SortedSetEntry> entries;
                    do
                    {
                        entries = db.SortedSetScan(channel_prefix + "_users", cursor: cursor?.Cursor ?? 0);
                        foreach (var entry in entries)
                        {
                            int id = int.Parse(entry.Element.ToString());

                            string chanmodeflags_update = string.Format("\\type\\UPDATE_USER_CHANMODEFLAGS\\to\\{0}\\user_id\\{1}\\modeflags\\0", channel_id, id);
                            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(chanmodeflags_update);

                            IBasicProperties props = channel.CreateBasicProperties();
                            props.ContentType = "text/plain";
                            channel.BasicPublish(PEERCHAT_EXCHANGE, PEERCHAT_KEYUPDATE_KEY, props, messageBodyBytes);


                            var kick_message = System.Text.Encoding.UTF8.GetBytes("Channel Reset");
                            
                            string kick_event_message = string.Format("\\type\\KICK\\toChannelId\\{0}\\fromUserSummary\\{1}\\toUserId\\{2}\\message\\{3}", channel_id, "SERVER!SERVER@0.0.0.0", id, Convert.ToBase64String(kick_message));
                            messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(kick_event_message);
                            channel.BasicPublish(PEERCHAT_EXCHANGE, PEERCAHT_CLIENT_MESSAGE_KEY, props, messageBodyBytes);
                        }

                        cursor = (IScanningCursor)entries;
                    } while ((cursor?.Cursor ?? 0) != 0);
                }
            }
            db.KeyDelete(channel_prefix);
            db.KeyDelete(channel_prefix + "_users");
            db.HashDelete("channels", channel_id.ToString());
        }
    }
}