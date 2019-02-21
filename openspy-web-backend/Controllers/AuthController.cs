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
            public String session_key;
        };
        public class AuthRequest
        {
            public ProfileLookup profileLookup;
            public UserLookup userLookup;
            public String password;
            public int? expiresInSecs;
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
            if (user == null) throw new NoSuchUserException();

            Profile profile = null;
            if (request.profileLookup != null)
            {
                profile = (await profileRepository.Lookup(request.profileLookup)).FirstOrDefault();
                if (profile == null) throw new NoSuchUserException();

                if (request.userLookup == null)
                {
                    request.userLookup = new UserLookup();
                }
                request.userLookup.id = profile.Userid;
            }


            Session sessionModel = new Session();
            sessionModel.profile = profile;
            sessionModel.user = user;
            if (request.expiresInSecs.HasValue)
            {
                sessionModel.expiresIn = TimeSpan.FromSeconds(request.expiresInSecs.Value);
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
            if (user == null) throw new NoSuchUserException();

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
            }


            if(request.password.CompareTo(user.Password) != 0)
            {
                throw new AuthInvalidCredentialsException();
            }

            response.profile = profile;
            response.user = user;

            Session session = new Session();
            session.profile = profile;
            session.user = user;
            if(request.expiresInSecs.HasValue)
                session.expiresIn = TimeSpan.FromSeconds(request.expiresInSecs.Value);
            session = await sessionRepository.Create(session);
            response.session_key = session.sessionKey;

            return response;
        }
    }
}