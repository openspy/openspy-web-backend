using CoreWeb.Database;
using CoreWeb.Models;
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
        private IMQConnectionFactory connectionFactory;
        private String GP_EXCHANGE;
        private String GP_BUDDY_ROUTING_KEY;
        private TimeSpan BUDDY_ADDREQ_EXPIRETIME;
        private PresenceStatusDatabase presenceStatusDatabase;

        public BuddyRepository(GameTrackerDBContext gameTrackerDb, IRepository<User, UserLookup> userRepository, IRepository<Profile, ProfileLookup> profileRepository, IMQConnectionFactory connectionFactory, PresenceStatusDatabase presenceStatusDatabase)
        {
            GP_EXCHANGE = "presence.core";
            GP_BUDDY_ROUTING_KEY = "presence.buddies";
            BUDDY_ADDREQ_EXPIRETIME = TimeSpan.FromSeconds(604800);

            this.userRepository = userRepository;
            this.profileRepository = profileRepository;
            this.gameTrackerDb = gameTrackerDb;
            this.connectionFactory = connectionFactory;
            this.presenceStatusDatabase = presenceStatusDatabase;
        }
        public async Task<IEnumerable<Buddy>> Lookup(BuddyLookup lookup)
        {
            var query = gameTrackerDb.Buddy as IQueryable<Buddy>;
            var from_profile = (await this.profileRepository.Lookup(lookup.SourceProfile)).First();
            query = query.Where(b => b.FromProfileid == from_profile.Id);
            var buddies = await query.ToListAsync();
            foreach(var buddy in buddies)
            {
                if(buddy.ToProfile == null)
                {
                    ProfileLookup plookup = new ProfileLookup();
                    plookup.id = buddy.ToProfileid;
                    buddy.ToProfile = (await this.profileRepository.Lookup(plookup)).First();
                }
            }
            return buddies;
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

        public async Task<bool> AuthorizeAdd(Profile from_profile, Profile to_profile)
        {
            if (DeleteBuddyRequest(to_profile, from_profile))
            {
                ConnectionFactory factory = connectionFactory.Get();
                Buddy buddy = new Buddy();
                buddy.ToProfileid = from_profile.Id;
                buddy.FromProfileid = to_profile.Id;
                await Create(buddy);
                using (IConnection connection = factory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        String message = String.Format("\\type\\authorize_add\\to_profileid\\{0}\\from_profileid\\{1}", to_profile.Id, from_profile.Id);
                        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                        IBasicProperties props = channel.CreateBasicProperties();
                        props.ContentType = "text/plain";
                        channel.BasicPublish(GP_EXCHANGE, GP_BUDDY_ROUTING_KEY, props, messageBodyBytes);
                        return true;
                    }
                }
            }/* else if(await IsOnBuddyList(to_profile, from_profile))
            {
                Buddy buddy = new Buddy();
                buddy.ToProfileid = to_profile.Id;
                buddy.FromProfileid = from_profile.Id;
                await Create(buddy);
                return true;
            }
            else
            {
                throw new ArgumentException();
            }*/
            return false;
        }
        public Task<bool> IsOnBuddyList(Profile from, Profile to)
        {
            return Task.Run(() =>
            {
                var query = gameTrackerDb.Buddy as IQueryable<Buddy>;
                query = query.Where(b => b.FromProfileid == from.Id && b.ToProfileid == to.Id);
                return query.Count() > 0;
            });
        }
        public bool DeleteBuddyRequest(Profile from, Profile to)
        {
            var redis_hash_key = "add_req_" + to.Id;
            var db = presenceStatusDatabase.GetDatabase();
            if(db.KeyExists(redis_hash_key))
            {
                db.KeyDelete(redis_hash_key);
                return true;
            }
            return false;
        }
        public Task SendAddEvent(Profile from, Profile to, String reason)
        {
            return Task.Run(() =>
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
            });
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
                    String message = String.Format("\\type\\{0}\\from_profileid\\{1}\\to_profileid\\{2}", type, from.Id, to.Id);
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.ContentType = "text/plain";
                    channel.BasicPublish(GP_EXCHANGE, GP_BUDDY_ROUTING_KEY, props, messageBodyBytes);
                }
            }
        }

        public async Task SendMessage(SendMessageRequest messageData)
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
        public async Task SendBuddyRequest(BuddyLookup lookupData)
        {
            ConnectionFactory factory = connectionFactory.Get();

            //check if buddy is added to list
            //if not, add to redis, sending MQ message
            var from_profile = (await this.profileRepository.Lookup(lookupData.SourceProfile)).First();
            var to_profile = (await this.profileRepository.Lookup(lookupData.TargetProfile)).First();
            var redis_hash_key = "add_req_" + to_profile.Id;

            var db = presenceStatusDatabase.GetDatabase();
            //redis.SetEntryInHash(redis_hash_key, from_profile.Id.ToString(), lookupData.addReason ?? "");
            //redis.ExpireEntryIn(redis_hash_key, BUDDY_ADDREQ_EXPIRETIME);
            db.HashSet(redis_hash_key, from_profile.Id.ToString(), lookupData.addReason ?? "");
            db.KeyExpire(redis_hash_key, BUDDY_ADDREQ_EXPIRETIME);

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
