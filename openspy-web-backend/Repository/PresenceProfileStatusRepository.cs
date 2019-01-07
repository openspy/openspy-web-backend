using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Models;
using RabbitMQ.Client;
using ServiceStack.Redis;

namespace CoreWeb.Repository
{
    public class PresenceProfileStatusRepository : IRepository<PresenceProfileStatus, PresenceProfileLookup>
    {
        private IRedisClientsManager redisClientManager;
        private IMQConnectionFactory connectionFactory;
        private IRepository<Profile, ProfileLookup> profileRepository;
        private int PRESENCE_STATUS_REDISDB;
        private String GP_EXCHANGE;
        private String GP_BUDDY_ROUTING_KEY;
        public PresenceProfileStatusRepository(IRedisClientsManager redisClientManager, IRepository<Profile, ProfileLookup> profileRepository, IMQConnectionFactory connectionFactory)
        {
            PRESENCE_STATUS_REDISDB = 5;
            GP_EXCHANGE = "presence.core";
            GP_BUDDY_ROUTING_KEY = "presence.buddies";
            this.profileRepository = profileRepository;
            this.redisClientManager = redisClientManager;
            this.connectionFactory = connectionFactory;
        }
        public async Task<IEnumerable<PresenceProfileStatus>> Lookup(PresenceProfileLookup lookup)
        {
            List<PresenceProfileStatus> list = new List<PresenceProfileStatus>();
            using (IRedisClient redis = redisClientManager.GetClient())
            {
                redis.Db = PRESENCE_STATUS_REDISDB;
                PresenceProfileStatus status = new PresenceProfileStatus();
                var to_profile = (await this.profileRepository.Lookup(lookup.profileLookup)).First();
                list.Add(GetStatusFromProfile(to_profile, redis));
            }
            return list;
        }
        private PresenceProfileStatus GetStatusFromProfile(Profile profile, IRedisClient redis)
        {
            PresenceProfileStatus status = new PresenceProfileStatus();
            var redis_hash_key = "status_" + profile.Id;
            status.IP = redis.GetValueFromHash(redis_hash_key, "address");
            ushort.TryParse(redis.GetValueFromHash(redis_hash_key, "port"), out status.Port);
            uint.TryParse(redis.GetValueFromHash(redis_hash_key, "status"), out status.statusFlags);
            uint.TryParse(redis.GetValueFromHash(redis_hash_key, "quiet_flags"), out status.quietFlags);
            status.statusText = redis.GetValueFromHash(redis_hash_key, "status_string");
            status.locationText = redis.GetValueFromHash(redis_hash_key, "location_string");
            status.profile = profile;
            return status;
        }
        public async Task<bool> Delete(PresenceProfileLookup lookup)
        {
            var to_profile = (await this.profileRepository.Lookup(lookup.profileLookup)).First();
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
                ConnectionFactory factory = connectionFactory.Get();
                using (IConnection connection = factory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        String message = String.Format("\\type\\status_update\\profileid\\{0}\\status_string\\{1}\\status\\{2}\\location_string\\{3}\\quiet_flags\\{4}\\ip\\{5}\\port\\{6}", to_profile.Id,
                    model.statusText, model.statusFlags, model.locationText, model.quietFlags, model.IP, model.Port);
                        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                        IBasicProperties props = channel.CreateBasicProperties();
                        props.ContentType = "text/plain";
                        channel.BasicPublish(GP_EXCHANGE, GP_BUDDY_ROUTING_KEY, props, messageBodyBytes);
                    }
                }
            }
            return model;
        }
        public Task<PresenceProfileStatus> Create(PresenceProfileStatus model)
        {
            throw new NotImplementedException();
        }
    }
}
