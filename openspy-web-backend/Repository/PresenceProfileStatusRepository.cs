using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Exception;
using CoreWeb.Models;
using RabbitMQ.Client;

namespace CoreWeb.Repository
{
    public class PresenceProfileStatusRepository : IRepository<PresenceProfileStatus, PresenceProfileLookup>
    {
        private PresenceStatusDatabase presenceStatusDatabase;
        private IMQConnectionFactory connectionFactory;
        private IRepository<Profile, ProfileLookup> profileRepository;
        private IRepository<User, UserLookup> userRepository;
        private IRepository<Buddy, BuddyLookup> buddyLookup;
        private IRepository<Block, BuddyLookup> blockLookup;
        private String GP_EXCHANGE;
        private String GP_BUDDY_ROUTING_KEY;
        private TimeSpan defaultTimeSpan;
        public PresenceProfileStatusRepository(PresenceStatusDatabase presenceStatusDatabase, IRepository<Profile, ProfileLookup> profileRepository, IMQConnectionFactory connectionFactory, IRepository<User, UserLookup> userRepository, IRepository<Buddy, BuddyLookup> buddyLookup, IRepository<Block, BuddyLookup> blockLookup)
        {
            GP_EXCHANGE = "presence.core";
            GP_BUDDY_ROUTING_KEY = "presence.buddies";
            this.profileRepository = profileRepository;
            this.userRepository = userRepository;
            this.buddyLookup = buddyLookup;
            this.blockLookup = blockLookup;
            this.presenceStatusDatabase = presenceStatusDatabase;
            this.connectionFactory = connectionFactory;
            this.defaultTimeSpan = TimeSpan.FromHours(2);
        }
        public async Task<IEnumerable<PresenceProfileStatus>> Lookup(PresenceProfileLookup lookup)
        {
            var is_buddy_lookup = lookup.buddyLookup.HasValue && lookup.buddyLookup.Value;
            var is_block_lookup = lookup.blockLookup.HasValue && lookup.blockLookup.Value;
            var is_reverse_lookup = lookup.reverseLookup.HasValue && lookup.reverseLookup.Value;

            var from_profile = (await this.profileRepository.Lookup(lookup.profileLookup)).FirstOrDefault();
            if (from_profile == null) throw new NoSuchUserException();

            List<PresenceProfileStatus> list = new List<PresenceProfileStatus>();
            PresenceProfileStatus status = new PresenceProfileStatus();
            if(!is_buddy_lookup && !is_block_lookup)
            {
                list.Add(await GetStatusFromProfile(from_profile));
            }
            else if (is_buddy_lookup)
            {
                BuddyLookup buddyLookup = new BuddyLookup();
                buddyLookup.SourceProfile = lookup.profileLookup;
                if(lookup.targetLookup != null)  {
                    buddyLookup.TargetProfile = lookup.targetLookup;
                }
                var buddies = (await this.buddyLookup.Lookup(buddyLookup));
                foreach (var buddy in buddies)
                {
                    list.Add(await GetStatusFromProfile(buddy.ToProfile));
                }
            }
            else if (is_block_lookup)
            {
                BuddyLookup buddyLookup = new BuddyLookup();
                buddyLookup.SourceProfile = lookup.profileLookup;
                var buddies = (await this.blockLookup.Lookup(buddyLookup));
                foreach (var buddy in buddies)
                {
                    list.Add(await GetStatusFromProfile(buddy.ToProfile));
                }
            }
            return list;
        }
        private async Task<PresenceProfileStatus> GetStatusFromProfile(Profile profile)
        {
            PresenceProfileStatus status = new PresenceProfileStatus();
            var redis_hash_key = "status_" + profile.Id;
            var db = presenceStatusDatabase.GetDatabase();
            status.IP = db.HashGet(redis_hash_key, "address");
            ushort.TryParse(db.HashGet(redis_hash_key, "port"), out status.Port);
            uint.TryParse(db.HashGet(redis_hash_key, "status"), out status.statusFlags);
            uint.TryParse(db.HashGet(redis_hash_key, "quiet_flags"), out status.quietFlags);
            status.statusText = db.HashGet(redis_hash_key, "status_string");
            status.locationText = db.HashGet(redis_hash_key, "location_string");
            status.profile = profile;

            var userLookup = new UserLookup();
            userLookup.id = profile.Userid;
            var user = (await this.userRepository.Lookup(userLookup)).FirstOrDefault();
            status.user = user;
            return status;
        }
        public async Task<bool> Delete(PresenceProfileLookup lookup)
        {
            var to_profile = (await this.profileRepository.Lookup(lookup.profileLookup)).FirstOrDefault();
            if (to_profile == null) return false;
            var redis_hash_key = "status_" + to_profile.Id;
            var db = presenceStatusDatabase.GetDatabase();
            ConnectionFactory factory = connectionFactory.Get();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    String message = String.Format("\\type\\status_update\\profileid\\{0}", to_profile.Id);
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/plain";
                    channel.BasicPublish(GP_EXCHANGE, GP_BUDDY_ROUTING_KEY, props, messageBodyBytes);
                }
            }
            db.KeyDelete(redis_hash_key);
            return true;
        }
        public async Task<PresenceProfileStatus> Update(PresenceProfileStatus model)
        {
            var to_profile = (await this.profileRepository.Lookup(model.profileLookup)).FirstOrDefault();
            if (to_profile == null) throw new NoSuchUserException();
            var redis_key = "status_" + to_profile.Id.ToString();
            var db = presenceStatusDatabase.GetDatabase();
            db.HashSet(redis_key, "address", model.IP.ToString());
            db.HashSet(redis_key, "port", model.Port.ToString());
            db.HashSet(redis_key, "status", model.statusFlags.ToString());
            db.HashSet(redis_key, "status_string", model.statusText);
            db.HashSet(redis_key, "location_string", model.locationText);
            db.HashSet(redis_key, "quiet_flags", model.quietFlags.ToString());
            db.KeyExpire(redis_key, this.defaultTimeSpan);
            await SendStatusUpdate(to_profile, model);
            return model;
        }
        public async Task SendStatusUpdate(Profile profile, PresenceProfileStatus status = null)
        {
            if(status == null)
            {
                status = await GetStatusFromProfile(profile);
                if (status == null)
                {
                    return;
                }
            }
            
            ConnectionFactory factory = connectionFactory.Get();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    String message = String.Format("\\type\\status_update\\profileid\\{0}\\status_string\\{1}\\status\\{2}\\location_string\\{3}\\quiet_flags\\{4}\\ip\\{5}\\port\\{6}", profile.Id,
                status.statusText, status.statusFlags, status.locationText, status.quietFlags, status.IP, status.Port);
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/plain";
                    channel.BasicPublish(GP_EXCHANGE, GP_BUDDY_ROUTING_KEY, props, messageBodyBytes);
                }
            }

        }
        public Task<PresenceProfileStatus> Create(PresenceProfileStatus model)
        {
            throw new NotImplementedException();
        }
    }
}
