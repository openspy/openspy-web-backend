using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Repository;
using CoreWeb.Models;

namespace CoreWeb.Controllers.Persist
{
    public class AuthRequest
    {
        public String client_response;
        public System.UInt32 session_key;
        /// <summary>
        /// User data to perform auth against (Used for Nick/Unique nick auth)
        /// </summary>
        public UserLookup user;
        /// <summary>
        /// Profile data to perform auth against  (Used for Nick/Unique nick auth)
        /// </summary>
        public ProfileLookup profile;
    }
    [Route("v1/Persist/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        IRepository<User, UserLookup> userRepository;
        IRepository<Profile, ProfileLookup> profileRepository;
        IRepository<Game, GameLookup> gameRepository;
        public AuthController(IRepository<User, UserLookup> userRepository, IRepository<Profile, ProfileLookup> profileRepository, IRepository<Game, GameLookup> gameRepository)
        {

            this.userRepository = userRepository;
            this.profileRepository = profileRepository;
            this.gameRepository = gameRepository;
        }
        [HttpPost("SessionKeyAuth")]
        public void SessionKeyAuth([FromBody] AuthRequest request)
        {

        }
        [HttpPost("PreAuth")]
        public void PreAuth([FromBody] AuthRequest request)
        {

        }
    }
}