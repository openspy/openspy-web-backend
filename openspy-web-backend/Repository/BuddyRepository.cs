using CoreWeb.Database;
using CoreWeb.Models;
using ServiceStack.Redis;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using RabbitMQ.Client;
using CoreWeb.Exception;

namespace CoreWeb.Repository
{
    public class BuddyRepository : IRepository<Buddy, BuddyLookup>
    {
        private GameTrackerDBContext gameTrackerDb;
        private IRepository<User, UserLookup> userRepository;
        private IRepository<Profile, ProfileLookup> profileRepository;
        private IRedisClientsManager redisClientManager;
        private IMQConnectionFactory connectionFactory;
        private int GP_REDIS_DB;
        private String GP_EXCHANGE;
        private String GP_BUDDY_ROUTING_KEY;
        private TimeSpan BUDDY_ADDREQ_EXPIRETIME;

        public BuddyRepository(GameTrackerDBContext gameTrackerDb, IRepository<User, UserLookup> userRepository, IRepository<Profile, ProfileLookup> profileRepository, IRedisClientsManager redisClientManager, IMQConnectionFactory connectionFactory)
        {
            GP_EXCHANGE = "presence.core";
            GP_BUDDY_ROUTING_KEY = "presence.buddies";
            GP_REDIS_DB = 5;
            BUDDY_ADDREQ_EXPIRETIME = TimeSpan.FromSeconds(604800);

            this.userRepository = userRepository;
            this.profileRepository = profileRepository;
            this.gameTrackerDb = gameTrackerDb;
            this.redisClientManager = redisClientManager;
            this.connectionFactory = connectionFactory;
        }
        public async Task<IEnumerable<Buddy>> Lookup(BuddyLookup lookup)
        {
            var query = gameTrackerDb.Buddy as IQueryable<Buddy>;
            var from_profile = (await this.profileRepository.Lookup(lookup.SourceProfile)).First();
            query = query.Where(b => b.FromProfileid == from_profile.Id);
            return await query.ToListAsync();
        }
        public Task<bool> Delete(BuddyLookup lookup)
        {
            return Task.Run(async () =>
            {
                var buddies = (await Lookup(lookup)).ToList();
                foreach (var buddy in buddies)
                {
                    gameTrackerDb.Remove<Buddy>(buddy);
                }
                var num_modified = await gameTrackerDb.SaveChangesAsync();
                return buddies.Count > 0 && num_modified > 0;
            });
        }
        public Task<Buddy> Update(Buddy model)
        {
            throw new NotImplementedException();
        }

        public async Task<Buddy> Create(Buddy model)
        {
            var entry = await gameTrackerDb.AddAsync<Buddy>(model);
            var num_modified = await gameTrackerDb.SaveChangesAsync();
            return entry.Entity;
        }
        public void SendAddBlock(Buddy model)
        {
            ConnectionFactory factory = connectionFactory.Get();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    String message = String.Format("\\type\\authorize_add\\from_profileid\\{0}\\to_profileid\\{1}", model.FromProfileid, model.ToProfileid);
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/plain";
                    channel.BasicPublish(GP_EXCHANGE, GP_BUDDY_ROUTING_KEY, props, messageBodyBytes);
                }
            }
        }

        public async void AuthorizeAdd(Profile from_profile, Profile to_profile)
        {
            if (DeleteBuddyRequest(from_profile, to_profile))
            {
                ConnectionFactory factory = connectionFactory.Get();
                Buddy buddy = new Buddy();
                buddy.ToProfileid = to_profile.Id;
                buddy.FromProfileid = from_profile.Id;
                await Create(buddy);
                using (IConnection connection = factory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        String message = String.Format("\\type\\authorize_add\\from_profileid\\{0}\\to_profileid\\{1}", from_profile.Id, to_profile.Id);
                        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                        IBasicProperties props = channel.CreateBasicProperties();
                        props.ContentType = "text/plain";
                        channel.BasicPublish(GP_EXCHANGE, GP_BUDDY_ROUTING_KEY, props, messageBodyBytes);
                    }
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }
        public bool DeleteBuddyRequest(Profile from, Profile to)
        {
            var redis_hash_key = "add_req_" + to.Id;
            using (IRedisClient redis = redisClientManager.GetClient())
            {
                redis.Db = GP_REDIS_DB;
                string reason = redis.GetValueFromHash(redis_hash_key, from.Id.ToString());
                if (reason == null)
                {
                    return false;
                }
                redis.Remove(redis_hash_key);
                return true;
            }
        }
        public void SendAddEvent(Profile from, Profile to, String reason)
        {
            ConnectionFactory factory = connectionFactory.Get();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    String message = String.Format("\\type\\add_request\\from_profileid\\{1}\\to_profileid\\{2}\\reason\\{0]", reason, from.Id, to.Id);
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/plain";
                    channel.BasicPublish(GP_EXCHANGE, GP_BUDDY_ROUTING_KEY, props, messageBodyBytes);
                }
            }
        }
        public void SendDeleteEvent(Profile from, Profile to)
        {
            SendBuddyEvent("del_buddy", from, to);
        }
        private void SendBuddyEvent(String type, Profile from, Profile to)
        {
            ConnectionFactory factory = connectionFactory.Get();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    String message = String.Format("\\type\\{0]}\\from_profileid\\{1}\\to_profileid\\{2}", type, from.Id, to.Id);
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/plain";
                    channel.BasicPublish(GP_EXCHANGE, GP_BUDDY_ROUTING_KEY, props, messageBodyBytes);
                }
            }
        }

        public async void SendMessage(SendMessageRequest messageData)
        {
            //TODO: check if user is online... if not save and send on next login
            ConnectionFactory factory = connectionFactory.Get();
            var from_profile = (await profileRepository.Lookup(messageData.lookup.SourceProfile)).FirstOrDefault();
            var to_profile = (await profileRepository.Lookup(messageData.lookup.TargetProfile)).FirstOrDefault();

            if (from_profile == null|| to_profile == null) throw new NoSuchUserException();
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    String message = String.Format("\\type\\buddy_message\\from_profileid\\{0}\\to_profileid\\{1}\\msg_type\\{2}\\message\\{3}", from_profile.Id, to_profile.Id, messageData.type, messageData.message);
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/plain";
                    channel.BasicPublish(GP_EXCHANGE, GP_BUDDY_ROUTING_KEY, props, messageBodyBytes);
                }
            }
        }
        public async void SendBuddyRequest(BuddyLookup lookupData)
        {
            ConnectionFactory factory = connectionFactory.Get();

            //check if buddy is added to list
            //if not, add to redis, sending MQ message
            var from_profile = (await this.profileRepository.Lookup(lookupData.SourceProfile)).First();
            var to_profile = (await this.profileRepository.Lookup(lookupData.TargetProfile)).First();
            var redis_hash_key = "add_req_" + to_profile.Id;

            using (IRedisClient redis = redisClientManager.GetClient())
            {
                redis.Db = GP_REDIS_DB;
                redis.SetEntryInHash(redis_hash_key, from_profile.Id.ToString(), lookupData.addReason ?? "");
                redis.ExpireEntryIn(redis_hash_key, BUDDY_ADDREQ_EXPIRETIME);

                using (IConnection connection = factory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        String message = String.Format("\\type\\add_request\\from_profileid\\{0}\\to_profileid\\{1}\\reason\\{2}", from_profile.Id, to_profile.Id, lookupData.addReason);
                        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                        IBasicProperties props = channel.CreateBasicProperties();
                        props.ContentType = "text/plain";
                        channel.BasicPublish(GP_EXCHANGE, GP_BUDDY_ROUTING_KEY, props, messageBodyBytes);
                    }
                }
            }
        }
    }
}
