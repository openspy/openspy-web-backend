using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Repository;
using CoreWeb.Models;
using CoreWeb.Exception;
using System.Security.Cryptography;
using System.Text;

namespace CoreWeb.Controllers.Persist
{
    public class AuthRequest
    {
        public String client_response;
        public String auth_token;
        public String cdkey;
        public System.Int32 session_key;
        /// <summary>
        /// Profile data to perform auth against  (Used for Nick/Unique nick auth)
        /// </summary>
        public ProfileLookup profileLookup;
    }
    public class AuthResponse
    {
        public Profile profile;
        public User user;
    };
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
        private string gs_sesskey(System.Int32 sesskey)
        {
            System.Int32 key = sesskey ^ 0x38f371e6;
            String s = key.ToString("x4");
            int offset = 17;
            String r = "";
            for (int i = 0; i < s.Length; i++)
            {
                byte ch = (byte)s[i];
                ch += (byte)offset;
                offset++;
                r += (char)ch;
            }
            return r;
        }

        [HttpPost("SessionKeyAuth")]
        public async Task<AuthResponse> SessionKeyAuth([FromBody] AuthRequest request)
        {
            var sesskey = gs_sesskey(request.session_key);
            var profile = (await profileRepository.Lookup(request.profileLookup)).FirstOrDefault();
            if (profile == null) throw new NoSuchUserException();
            var userLookup = new UserLookup();
            userLookup.id = profile.Userid;
            var user = (await userRepository.Lookup(userLookup)).FirstOrDefault();
            if (user == null) throw new NoSuchUserException();

            StringBuilder sb = new StringBuilder();

            sb.Append(user.Password);
            sb.Append(sesskey);
            String md5String;
            using (MD5 md5Hash = MD5.Create())
            {
                StringBuilder sBuilder = new StringBuilder();
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                md5String = sBuilder.ToString().ToLower();
            }
            if(md5String.Equals(request.client_response.ToLower()))
            {
                var resp = new AuthResponse();
                resp.profile = profile;
                resp.user = user;
                return resp;
            }
            throw new AuthInvalidCredentialsException();
        }

        /*
             def test_gstats_sessionkey_response_auth_token(self, request_body, account_data):
        response = {}

        token = request_body['auth_token']

        if not self.redis_ctx.exists("auth_token_{}".format(token)):
            raise OS_Auth_InvalidCredentials()

        profileid = int(self.redis_ctx.hget("auth_token_{}".format(token), 'profileid'))
        profile = self.get_profile_by_id(profileid)
        user = self.get_user_by_userid(profile.userid)
        challenge = self.redis_ctx.hget("auth_token_{}".format(token), 'challenge').decode('utf-8')
        challenge = str(challenge).encode('utf-8')

        sess_key = self.gs_sesskey(request_body["session_key"])

        pw_hashed = "{}{}".format(challenge,sess_key).encode('utf-8')
        pw_hashed = hashlib.md5(pw_hashed).hexdigest()

        response["success"] = pw_hashed == request_body["client_response"]       

        response['profile'] = model_to_dict(profile)
        response['user'] = model_to_dict(user)
        return response
         */
        [HttpPost("PreAuth")]
        public void PreAuth([FromBody] AuthRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("CDKeyAuth")]
        public async Task<AuthResponse> CDKeyAuth([FromBody] AuthRequest request)
        {
            var lookup = new ProfileLookup();
            lookup.id = 1;
            var profile = (await profileRepository.Lookup(lookup)).FirstOrDefault();
            if (profile == null) throw new NoSuchUserException();

            var response = new AuthResponse();
            response.profile = profile;
            var userLookup = new UserLookup();
            userLookup.id = profile.Userid;
            var user = (await userRepository.Lookup(userLookup)).FirstOrDefault();
            if (user == null) throw new NoSuchUserException();
            response.user = user;
            return response;
        }
        [HttpPost("ProfileFromCDKey")]
        public async Task<AuthResponse> ProfileFromCDKey([FromBody] AuthRequest request)
        {
            var lookup = new ProfileLookup();
            lookup.id = 1;
            var profile = (await profileRepository.Lookup(lookup)).FirstOrDefault();
            if (profile == null) throw new NoSuchUserException();

            var response = new AuthResponse();
            response.profile = profile;
            var userLookup = new UserLookup();
            userLookup.id = profile.Userid;
            var user = (await userRepository.Lookup(userLookup)).FirstOrDefault();
            if (user == null) throw new NoSuchUserException();
            response.user = user;
            return response;
        }
    }
}