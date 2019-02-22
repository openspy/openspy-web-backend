using CoreWeb.Database;
using CoreWeb.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CoreWeb.Crypto;

namespace CoreWeb.Repository
{
    public class AuthSessionRepository : IRepository<Session, SessionLookup>
    {
        private const int REDIS_SESSION_DB = 3;
        private IRepository<User, UserLookup> userRepository;
        private IRepository<Profile, ProfileLookup> profileRepository;
        private IRepository<Game, GameLookup> gameRepository;
        private IMQConnectionFactory mqConnectionFactory;
        private PresencePreAuthProvider rsaProvider;
        private TimeSpan defaultTimeSpan;
        private SessionCacheDatabase sessionCache;
        public AuthSessionRepository(SessionCacheDatabase sessionCache, IRepository<User, UserLookup> userRepository, IRepository<Profile, ProfileLookup> profileRepository, IRepository<Game, GameLookup> gameRepository, IMQConnectionFactory mqConnectionFactory, PresencePreAuthProvider rsaProvider)
        {
            this.defaultTimeSpan = TimeSpan.FromHours(2);
            this.userRepository = userRepository;
            this.profileRepository = profileRepository;
            this.gameRepository = gameRepository;
            this.mqConnectionFactory = mqConnectionFactory;
            this.rsaProvider = rsaProvider;
            this.sessionCache = sessionCache;
        }
        public Task<IEnumerable<Session>> Lookup(SessionLookup lookup)
        {
            return null;
        }
        public Task<bool> Delete(SessionLookup lookup)
        {
            return Task.Run(() =>
            {
                return false;
            });            
        }
        public Task<Session> Update(Session model) {
            return Task.Run(() =>
            {
                return (Session)null;
            });    
        }

        public async Task<Session> Create(Session model)
        {
            Session session = new Session();
            var session_key = generateSessionKey();
            var db = sessionCache.GetDatabase();
            db.HashSet(session_key.ToString(), "guid", session_key.ToString());
            if (model.profile != null && model.profile.Id != 0)
            {
                db.HashSet(session_key.ToString(), "profileid", model.profile.Id.ToString());
                db.HashSet(session_key.ToString(), "userid", model.profile.Userid.ToString());
            }
            else if (model.user != null)
            {
                UserLookup userLookup = new UserLookup();
                if (model.user.Id != 0)
                {
                    userLookup.id = model.user.Id;
                    db.HashSet(session_key.ToString(), "userid", model.user.Id.ToString());
                }
                session.user = (await this.userRepository.Lookup(userLookup)).ToList().First();
            }
            else
            {
                throw new ArgumentException();
            }

            if (model.profile != null)
            {
                ProfileLookup lookup = new ProfileLookup();
                lookup.id = model.profile.Id;
                session.profile = (await this.profileRepository.Lookup(lookup)).ToList().First();
            }

            session.expiresIn = model.expiresIn ?? this.defaultTimeSpan;
            session.expiresAt = DateTime.Now.Add(model.expiresIn ?? this.defaultTimeSpan);
            session.sessionKey = session_key;
            db.KeyExpire(session_key.ToString(), model.expiresIn ?? this.defaultTimeSpan);
            SendLoginEvent();
            return session;
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

        public Task<Dictionary<string, string>> decodeAuthToken(String token)
        {
            return Task.Run(async () =>
            {
                byte[] rawData = Convert.FromBase64String(token);
                var json_buff = Encoding.ASCII.GetString(rawData);
                Dictionary<string, string> tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json_buff);

                tokenData["true_signature"] = await generateTokenSignature(rawData);

                return tokenData;
            });
        }
        public Task<Tuple<String, String>> generateAuthToken(Profile profile, DateTime? expiresAt)
        {
            return Task.Run(async() =>
            {
                Dictionary<string, string> authData = new Dictionary<string, string>();

                authData["profileId"] = profile.Id.ToString();
                authData["userId"] = profile.Userid.ToString();
                if (expiresAt.HasValue)
                    authData["expiresAt"] = expiresAt.Value.ToFileTimeUtc().ToString();

                var json_buff = JsonConvert.SerializeObject(authData);
                var json_bytes = Encoding.UTF8.GetBytes(json_buff);
                  
                var auth_token = Convert.ToBase64String(json_bytes);
                var challenge = await generateTokenSignature(json_bytes);

                return new Tuple<String, String>(auth_token, challenge);
            });
            
        }
        public Task<String> generateTokenSignature(byte[] token)
        {
            return Task.Run(() =>
            {
                var signature = this.rsaProvider.Sign(token);
                return Convert.ToBase64String(signature);
            });
        }
    }
}
