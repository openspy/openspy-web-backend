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
        public GameLookup gameLookup;
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
        AuthSessionRepository sessionRepository;
        private CdKeyRepository cdkeyRepository;
        public AuthController(IRepository<User, UserLookup> userRepository, IRepository<Profile, ProfileLookup> profileRepository, IRepository<Game, GameLookup> gameRepository, IRepository<Session, SessionLookup> sessionRepository, IRepository<CdKey, CdKeyLookup> cdkeyRepository)
        {

            this.userRepository = userRepository;
            this.profileRepository = profileRepository;
            this.gameRepository = gameRepository;
            this.sessionRepository = (AuthSessionRepository)sessionRepository;
            this.cdkeyRepository = (CdKeyRepository)cdkeyRepository;
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

        [HttpPost("ProfileIDAuth")]
        public async Task<AuthResponse> ProfileIDAuth([FromBody] AuthRequest request)
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

        [HttpPost("PreAuth")]
        public async Task<AuthResponse> PreAuth([FromBody] AuthRequest request)
        {
            Dictionary<string, string> dict = await sessionRepository.decodeAuthToken(request.auth_token);
            if (dict == null) throw new AuthInvalidCredentialsException();
            var response = new AuthResponse();
            ProfileLookup profileLookup = new ProfileLookup();
            UserLookup userLookup = new UserLookup();
            int profileId;

            int.TryParse(dict["profileId"], out profileId);
            profileLookup.id = profileId;


            User user = null;
            if (dict.ContainsKey("userId"))
            {
                int.TryParse(dict["userId"], out profileId);
                userLookup.id = profileId;
                user = (await userRepository.Lookup(userLookup)).First();
            }

            response.profile = (await profileRepository.Lookup(profileLookup)).First();

            if (user == null)
            {
                userLookup.id = response.profile.Userid;
                user = (await userRepository.Lookup(userLookup)).First();
            }

            response.user = user;


            var sesskey = gs_sesskey(request.session_key);

            string challenge = dict["true_signature"] + sesskey.ToString();
            using(MD5 md5 = MD5.Create())
            {
                StringBuilder sBuilder = new StringBuilder();
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(challenge));
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                challenge = sBuilder.ToString().ToLower();
            }

            if (!challenge.Equals(request.client_response.ToLower()))
            {
                throw new AuthInvalidCredentialsException();
            }

            return response;
        }

        [HttpPost("CDKeyAuth")]
        public async Task<AuthResponse> CDKeyAuth([FromBody] AuthRequest request)
        {
            var response = new AuthResponse();
            var cdKeyLookup = new CdKeyLookup();
            cdKeyLookup.CdkeyHash = request.cdkey;
            if(request.gameLookup == null) throw new AuthInvalidCredentialsException();
            cdKeyLookup.Gameid = request.gameLookup.id;
            var profile = await cdkeyRepository.LookupProfileFromCDKey(cdKeyLookup);
            if(profile == null || request.profileLookup == null || profile.Nick.CompareTo(request.profileLookup.nick) != 0)
            {
                throw new AuthInvalidCredentialsException();
            }
            response.profile = profile;
            var userLookup = new UserLookup();
            userLookup.id = profile.Userid;
            var user = (await userRepository.Lookup(userLookup)).FirstOrDefault();
            response.user = user;

            var sesskey = gs_sesskey(request.session_key);

            var cdkey = (await cdkeyRepository.Lookup(cdKeyLookup)).FirstOrDefault();
            string challenge = cdkey.Cdkey + sesskey.ToString();
            using (MD5 md5 = MD5.Create())
            {
                StringBuilder sBuilder = new StringBuilder();
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(challenge));
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                challenge = sBuilder.ToString().ToLower();
            }

            if (!challenge.Equals(request.client_response.ToLower()))
            {
                throw new AuthInvalidCredentialsException();
            }
            return response;
        }
        [HttpPost("ProfileFromCDKey")]
        public async Task<AuthResponse> ProfileFromCDKey([FromBody] AuthRequest request)
        {
            var response = new AuthResponse();
            var cdKeyLookup = new CdKeyLookup();
            cdKeyLookup.CdkeyHash = request.cdkey;
            if (request.gameLookup == null) throw new BadCdKeyException();
            cdKeyLookup.Gameid = request.gameLookup.id;
            var profile = await cdkeyRepository.LookupProfileFromCDKey(cdKeyLookup);
            if (profile == null || request.profileLookup == null || profile.Nick.CompareTo(request.profileLookup.nick) != 0)
            {
                throw new BadCdKeyException();
            }
            response.profile = profile;
            var userLookup = new UserLookup();
            userLookup.id = profile.Userid;
            var user = (await userRepository.Lookup(userLookup)).FirstOrDefault();
            response.user = user;
            return response;
        }
    }
}