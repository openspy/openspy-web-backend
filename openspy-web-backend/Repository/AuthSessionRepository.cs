using CoreWeb.Database;
using CoreWeb.Models;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace CoreWeb.Repository
{
    public class SessionLookup
    {
        public String key;
        public ProfileLookup profile;
    };
    public class Session
    {
        public Profile profile;
        public TimeSpan ?expiresIn;
        public DateTime? expiresAt;
        public String sessionKey;
    };
    public class AuthSessionRepository : IRepository<Session, SessionLookup>
    {
        private const int REDIS_SESSION_DB = 3;
        private IRedisClientsManager redisClientManager;
        private IRepository<User, UserLookup> userRepository;
        private IRepository<Profile, ProfileLookup> profileRepository;
        private IRepository<Game, GameLookup> gameRepository;
        private IMQConnectionFactory mqConnectionFactory;
        private TimeSpan defaultTimeSpan;
        public AuthSessionRepository(IRedisClientsManager redisClientManager, IRepository<User, UserLookup> userRepository, IRepository<Profile, ProfileLookup> profileRepository, IRepository<Game, GameLookup> gameRepository, IMQConnectionFactory mqConnectionFactory)
        {
            this.defaultTimeSpan = TimeSpan.FromHours(2);
            this.redisClientManager = redisClientManager;
            this.userRepository = userRepository;
            this.profileRepository = profileRepository;
            this.gameRepository = gameRepository;
            this.mqConnectionFactory = mqConnectionFactory;
        }
        public Task<IEnumerable<Session>> Lookup(SessionLookup lookup)
        {
            return null;
        }
        public Task<bool> Delete(SessionLookup lookup)
        {
            return null;
        }
        public Task<Session> Update(Session model)
        {
            return null;
        }
        public Task<Session> Create(Session model)
        {
            return Task.Run(async () =>
            {
                Session session = new Session();
                using (IRedisClient redis = redisClientManager.GetClient())
                {
                    var session_key = generateSessionKey();
                    redis.Db = REDIS_SESSION_DB;
                    redis.SetEntryInHash(session_key, "guid", session_key);
                    if (model.profile != null && model.profile.Id != 0)
                    {
                        redis.SetEntryInHash(session_key, "profileid", model.profile.Id.ToString());
                        redis.SetEntryInHash(session_key, "userid", model.profile.User.Id.ToString());
                    } else
                    {
                        throw new ArgumentException();
                    }

                    ProfileLookup lookup = new ProfileLookup();
                    lookup.id = model.profile.Id;
                    session.profile = (await this.profileRepository.Lookup(lookup)).ToList().First();
                    session.expiresIn = model.expiresIn ?? this.defaultTimeSpan;
                    session.expiresAt = DateTime.Now.Add(model.expiresIn ?? this.defaultTimeSpan);
                    session.sessionKey = session_key;
                    redis.ExpireEntryIn(session_key, model.expiresIn ?? this.defaultTimeSpan);
                    SendLoginEvent();
                }
                return session;
            });
        }
        private String generateSessionKey()
        {
            StringBuilder sb = new StringBuilder();
            String md5String;
            var session_key = Guid.NewGuid().ToString();
            using (MD5 md5Hash = MD5.Create())
            {
                StringBuilder sBuilder = new StringBuilder();
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(session_key));
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                md5String = sBuilder.ToString();
            }
            return md5String;
        }
        private void SendLoginEvent()
        {
            //post MQ message with peer app name/addr
        }
    }
}
