using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Models;
using CoreWeb.Repository;
using CoreWeb.Exception;

namespace CoreWeb.Controllers
{
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

        IRepository<User, UserLookup> userRepository;
        IRepository<Profile, ProfileLookup> profileRepository;
        IRepository<Session, SessionLookup> sessionRepository;

        public AuthController(IRepository<User, UserLookup> userRepository, IRepository<Profile, ProfileLookup> profileRepository, IRepository<Session, SessionLookup> sessionRepository)
        {
            this.profileRepository = profileRepository;
            this.userRepository = userRepository;
            this.sessionRepository = sessionRepository;
        }
        [HttpPost("Login")]
        public async Task<AuthResponse> PostLogin([FromBody] AuthRequest request)
        {
            AuthResponse response = new AuthResponse();

            Profile profile = null;
            if(request.profileLookup != null)
            {
                profile = (await profileRepository.Lookup(request.profileLookup)).First();
                if (profile == null) throw new AuthNoSuchUserException();

                if(request.userLookup == null)
                {
                    request.userLookup = new UserLookup();
                }
                request.userLookup.id = profile.Userid;
            }

            User user = null;
            if (request.userLookup != null)
            {
                user = (await userRepository.Lookup(request.userLookup)).First();
                if (request.profileLookup != null)
                {
                    request.profileLookup.userId = user.Id;
                }
            }
            if (user == null) throw new AuthNoSuchUserException();

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