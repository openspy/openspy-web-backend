using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Models;
using CoreWeb.Repository;
using CoreWeb.Exception;
using Microsoft.AspNetCore.Authorization;
using CoreWeb.Filters;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace CoreWeb.Controllers
{
    [Authorize(Policy = "UserAuth")]
    [Route("v1/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        public class AuthResponse
        {
            public Profile profile;
            public User user;
            public Session session;
        };
        public class AuthRequest
        {
            public ProfileLookup profileLookup;
            public UserLookup userLookup;
            public String password;
            [JsonConverter(typeof(JsonTimeSpanConverter))]
            public TimeSpan? expiresIn;
        };

        public class SessionDeleteResponse
        {
            public bool success;
        }

        IRepository<User, UserLookup> userRepository;
        IRepository<Profile, ProfileLookup> profileRepository;
        IRepository<Session, SessionLookup> sessionRepository;

        public AuthController(IRepository<User, UserLookup> userRepository, IRepository<Profile, ProfileLookup> profileRepository, IRepository<Session, SessionLookup> sessionRepository)
        {
            this.profileRepository = profileRepository;
            this.userRepository = userRepository;
            this.sessionRepository = sessionRepository;
        }
        [HttpPut("Session")]
        public async Task<Session> GenerateSession([FromBody]AuthRequest request)
        {
            Profile profile = null;
            if (request.profileLookup != null)
            {
                profile = (await profileRepository.Lookup(request.profileLookup)).FirstOrDefault();
                if (profile == null) throw new NoSuchUserException();
            }

            var userLookup = new UserLookup();
            userLookup.id = profile.Userid;
            User user = (await userRepository.Lookup(userLookup)).FirstOrDefault();
             
            if (user == null) throw new NoSuchUserException();


            Session sessionModel = new Session();
            sessionModel.profile = profile;
            sessionModel.user = user;
            if (request.expiresIn.HasValue)
            {
                sessionModel.expiresIn = request.expiresIn.Value;
            }
            return await this.sessionRepository.Create(sessionModel);
        }
        [HttpDelete("Session")]
        public async Task<SessionDeleteResponse> DeleteSession([FromBody]SessionLookup request)
        {
            var resp = new SessionDeleteResponse();
            resp.success = await this.sessionRepository.Delete(request);
            return resp;
        }
        [HttpPost("Login")]
        public async Task<AuthResponse> PostLogin([FromBody] AuthRequest request)
        {
            AuthResponse response = new AuthResponse();

            User user = null;
            if (request.userLookup != null)
            {
                user = (await userRepository.Lookup(request.userLookup)).FirstOrDefault();
                if (request.profileLookup != null)
                {
                    var userLookup = new UserLookup();
                    userLookup.id = user.Id;
                    request.profileLookup.user = userLookup;
                }                
            }

            Profile profile = null;
            if(request.profileLookup != null)
            {
                profile = (await profileRepository.Lookup(request.profileLookup)).FirstOrDefault();
                if (profile == null) throw new NoSuchUserException();

                if(request.userLookup == null)
                {
                    request.userLookup = new UserLookup();
                }
                request.userLookup.id = profile.Userid;
                if(user == null)
                {
                    user = (await userRepository.Lookup(request.userLookup)).FirstOrDefault();
                    if (user == null) throw new NoSuchUserException();
                }
                
            }


            if(request.password.CompareTo(user.Password) != 0)
            {
                /*Check MD5 string, for peerchat support (md5 hash is sent instead)*/
                String md5String;
                using (MD5 md5Hash = MD5.Create())
                {
                    StringBuilder sBuilder = new StringBuilder();
                    byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(user.Password));
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }
                    md5String = sBuilder.ToString();
                }
                if(request.password.CompareTo(md5String) != 0)
                {
                    throw new AuthInvalidCredentialsException();
                }
                
            }

            response.profile = profile;
            response.user = user;

            Session session = new Session();
            session.profile = profile;
            session.user = user;
            if(request.expiresIn.HasValue)
                session.expiresIn = request.expiresIn;
            response.session = await sessionRepository.Create(session);

            return response;
        }

        [HttpPost("GetSession")]
        public async Task<Session> GetSession([FromBody] AuthRequest request)
        {
            var lookup = new SessionLookup();
            lookup.sessionKey = request.password;
            var session = (await sessionRepository.Lookup(lookup)).FirstOrDefault();
            return session;
        }

        [HttpPost("TestPreAuth")]
        public async Task<AuthResponse> TestPreAuth([FromBody] AuthTicketData request)
        {
            
            var auth_data = (await ((AuthSessionRepository)sessionRepository).decodeAuthToken(request.token));
            String true_sig = auth_data["true_signature"].ToString();
            if(!true_sig.Equals(request.challenge)) {
                throw new AuthInvalidCredentialsException();
            }
            AuthResponse response = new AuthResponse();
            int profileid = 0, userid = 0;
            if(int.TryParse(auth_data["profileId"].ToString(), out profileid)) {
                response.profile = (await profileRepository.Lookup(new ProfileLookup { id = profileid})).FirstOrDefault();
                if(response.profile == null) {
                    throw new NoSuchUserException();
                }
            }

            if(int.TryParse(auth_data["userId"].ToString(), out userid)) {
                response.user = (await userRepository.Lookup(new UserLookup { id = userid})).FirstOrDefault();
                if(response.user == null) {
                    throw new NoSuchUserException();
                }
            }

            Session session = new Session();
            session.profile = response.profile;
            session.user = response.user;

            int expiresAt = 0;
            if(int.TryParse(auth_data["expiresAt"].ToString(), out expiresAt)) {
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(expiresAt).ToLocalTime();
                session.expiresAt = dtDateTime;
                response.session = await sessionRepository.Create(session);
            }
            
            
            return response;
            
        }

        //
    }
}