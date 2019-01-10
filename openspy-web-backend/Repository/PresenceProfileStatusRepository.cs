using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Exception;
using CoreWeb.Models;
using RabbitMQ.Client;
using ServiceStack.Redis;
using CoreWeb.Exception;

namespace CoreWeb.Repository
{
    public class PresenceProfileStatusRepository : IRepository<PresenceProfileStatus, PresenceProfileLookup>
    {
        private IRedisClientsManager redisClientManager;
        private IMQConnectionFactory connectionFactory;
        private IRepository<Profile, ProfileLookup> profileRepository;
        private IRepository<User, UserLookup> userRepository;
        private IRepository<Buddy, BuddyLookup> buddyLookup;
        private IRepository<Block, BuddyLookup> blockLookup;
        private int PRESENCE_STATUS_REDISDB;
        private String GP_EXCHANGE;
        private String GP_BUDDY_ROUTING_KEY;
        public PresenceProfileStatusRepository(IRedisClientsManager redisClientManager, IRepository<Profile, ProfileLookup> profileRepository, IMQConnectionFactory connectionFactory, IRepository<User, UserLookup> userRepository, IRepository<Buddy, BuddyLookup> buddyLookup, IRepository<Block, BuddyLookup> blockLookup)
        {
            PRESENCE_STATUS_REDISDB = 5;
            GP_EXCHANGE = "presence.core";
            GP_BUDDY_ROUTING_KEY = "presence.buddies";
            this.profileRepository = profileRepository;
            this.userRepository = userRepository;
            this.buddyLookup = buddyLookup;
            this.blockLookup = blockLookup;
            this.redisClientManager = redisClientManager;
            this.connectionFactory = connectionFactory;
        }
        public async Task<IEnumerable<PresenceProfileStatus>> Lookup(PresenceProfileLookup lookup)
        {
            var is_buddy_lookup = lookup.buddyLookup.HasValue && lookup.buddyLookup.Value;
            var is_block_lookup = lookup.blockLookup.HasValue && lookup.blockLookup.Value;
            var is_reverse_lookup = lookup.reverseLookup.HasValue && lookup.reverseLookup.Value;

            var from_profile = (await this.profileRepository.Lookup(lookup.profileLookup)).FirstOrDefault();
            if (from_profile == null) throw new NoSuchUserException();

            List<PresenceProfileStatus> list = new List<PresenceProfileStatus>();
            using (IRedisClient redis = redisClientManager.GetClient())
            {
                redis.Db = PRESENCE_STATUS_REDISDB;
                PresenceProfileStatus status = new PresenceProfileStatus();
                if(!is_buddy_lookup && !is_block_lookup)
                {
                    list.Add(await GetStatusFromProfile(from_profile, redis));
                }
                else if (is_buddy_lookup)
                {
                    BuddyLookup buddyLookup = new BuddyLookup();
                    buddyLookup.SourceProfile = lookup.profileLookup;
                    var buddies = (await this.buddyLookup.Lookup(buddyLookup));
                    foreach (var buddy in buddies)
                    {
                        list.Add(await GetStatusFromProfile(buddy.ToProfile, redis));
                    }
                }
                else if (is_block_lookup)
                {
                    BuddyLookup buddyLookup = new BuddyLookup();
                    buddyLookup.SourceProfile = lookup.profileLookup;
                    var buddies = (await this.blockLookup.Lookup(buddyLookup));
                    foreach (var buddy in buddies)
                    {
                        list.Add(await GetStatusFromProfile(buddy.ToProfile, redis));
                    }
                }

            }
            return list;
        }
        private async Task<PresenceProfileStatus> GetStatusFromProfile(Profile profile, IRedisClient redis)
        {
            redis.Db = PRESENCE_STATUS_REDISDB;
            PresenceProfileStatus status = new PresenceProfileStatus();
            var redis_hash_key = "status_" + profile.Id;
            status.IP = redis.GetValueFromHash(redis_hash_key, "address");
            ushort.TryParse(redis.GetValueFromHash(redis_hash_key, "port"), out status.Port);
            uint.TryParse(redis.GetValueFromHash(redis_hash_key, "status"), out status.statusFlags);
            uint.TryParse(redis.GetValueFromHash(redis_hash_key, "quiet_flags"), out status.quietFlags);
            status.statusText = redis.GetValueFromHash(redis_hash_key, "status_string");
            status.locationText = redis.GetValueFromHash(redis_hash_key, "location_string");
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
            using (IRedisClient redis = redisClientManager.GetClient())
            {
                redis.Db = PRESENCE_STATUS_REDISDB;
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
                redis.Remove(redis_hash_key);
            }
            return true;
        }
        public async Task<PresenceProfileStatus> Update(PresenceProfileStatus model)
        {
            var to_profile = (await this.profileRepository.Lookup(model.profileLookup)).FirstOrDefault();
            if (to_profile == null) throw new NoSuchUserException();
            using (IRedisClient redis = redisClientManager.GetClient())
            {
                redis.Db = PRESENCE_STATUS_REDISDB;
                var redis_key = "status_" + to_profile.Id.ToString();
                redis.SetEntryInHash(redis_key, "address", model.IP.ToString());
                redis.SetEntryInHash(redis_key, "port", model.Port.ToString());
                redis.SetEntryInHash(redis_key, "status", model.statusFlags.ToString());
                redis.SetEntryInHash(redis_key, "status_string", model.statusText);
                redis.SetEntryInHash(redis_key, "location_string", model.locationText);
                redis.SetEntryInHash(redis_key, "quiet_flags", model.quietFlags.ToString());
                await SendStatusUpdate(to_profile, model);
            }
            return model;
        }
        public async Task SendStatusUpdate(Profile profile, PresenceProfileStatus status = null)
        {
            if(status == null)
            {
                using (IRedisClient redis = redisClientManager.GetClient())
                {
                    status = await GetStatusFromProfile(profile, redis);
                }
                if(status == null)
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
