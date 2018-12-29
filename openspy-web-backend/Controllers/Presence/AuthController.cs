using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Models;
using CoreWeb.Repository;
using CoreWeb.Exception;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreWeb.Controllers.Presence
{
    public class AuthRequest
    {
        /// <summary>
        /// GPCM generated server challenges
        /// </summary>
        public String client_challenge;
        public String server_challenge;
        /// <summary>
        /// GP SDK challenge response
        /// </summary>
        public String client_response;
        /// <summary>
        /// Auth token to be used for preauth
        /// </summary>
        public String auth_token;

        /// <summary>
        /// User data to perform auth against (Used for Nick/Unique nick auth)
        /// </summary>
        public UserLookup user;
        /// <summary>
        /// Profile data to perform auth against  (Used for Nick/Unique nick auth)
        /// </summary>
        public ProfileLookup profile;
    };
    public class AuthResponse
    {
        public String proof;
        public Profile profile;
        public String server_response;
        public String session_key;
        public bool success;
    };
    public class GenAuthTicketResponse {
        public String token;
        public String challenge;
    };

    enum ProofType
    {
        ProofType_NickEmail,
        ProofType_Unique,
        ProofType_PreAuth,
    };


    [Route("v1/Presence/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        const String PROOF_BIG_SPACE = "                                                ";
        IRepository<User, UserLookup> userRepository;
        IRepository<Profile, ProfileLookup> profileRepository;
        IRepository<Game, GameLookup> gameRepository;
        IRepository<Session, SessionLookup> sessionRepository;
        public AuthController(IRepository<User, UserLookup> userRepository, IRepository<Profile, ProfileLookup> profileRepository, IRepository<Game, GameLookup> gameRepository, IRepository<Session, SessionLookup> sessionRepository)
        {
            this.userRepository = userRepository;
            this.profileRepository = profileRepository;
            this.gameRepository = gameRepository;
            this.sessionRepository = sessionRepository;
        }
        [HttpPost("GenAuthTicket")]
        public GenAuthTicketResponse GenAuthTicket([FromBody] AuthRequest authRequest)
        {
            throw new NotImplementedException();
        }
        [HttpPost("PreAuth")]
        public AuthResponse PreAuth([FromBody] AuthRequest authRequest)
        {
            throw new NotImplementedException();
        }
        [HttpPost("NickEmailAuth")]
        public Task<AuthResponse> NickEmailAuth([FromBody] AuthRequest authRequest)
        {
            return handleAuthRequest(authRequest, ProofType.ProofType_NickEmail);
        }
        [HttpPost("UniqueNickAuth")]
        public Task<AuthResponse> UniqueNickAuth([FromBody] AuthRequest authRequest)
        {
            return handleAuthRequest(authRequest, ProofType.ProofType_Unique);
        }

        private async Task<AuthResponse> handleAuthRequest(AuthRequest authRequest, ProofType type)
        {
            AuthResponse response = new AuthResponse();
            var user = (await userRepository.Lookup(authRequest.user)).First();
            if (user == null) throw new AuthNoSuchUserException();
            if (authRequest.profile != null)
            {
                authRequest.profile.userId = user.Id;
            }
            var profile = (await profileRepository.Lookup(authRequest.profile)).First();
            if (profile == null) throw new AuthNoSuchUserException();

            String client_proof = GetPasswordProof(profile, authRequest, type, true);


            if (!client_proof.Equals(authRequest.client_response))
            {
                throw new AuthInvalidCredentialsException();
            }

            response.server_response = GetPasswordProof(profile, authRequest, type, false);
            response.profile = profile;
            response.session_key = await generateSessionKey(profile);
            response.success = true;
            return response;
        }

        private String GetPasswordProof(Profile profile, AuthRequest request, ProofType type, bool client_to_server)
        {
            List<String> challenges = new List<String>();
            StringBuilder sb = new StringBuilder();
            var password = profile.User.Password;
            switch (type)
            {
                case ProofType.ProofType_NickEmail:
                    if(profile.User.Partnercode != CoreWeb.Models.User.PARTNERID_GAMESPY)
                    {
                        sb.Append(profile.User.Partnercode.ToString());
                        sb.Append("@");
                        sb.Append(profile.Nick);
                        sb.Append("@");
                        sb.Append(profile.User.Email);
                    }
                    else
                    {
                        sb.Append(profile.Nick);
                        sb.Append("@");
                        sb.Append(profile.User.Email);
                    }
                break;
                case ProofType.ProofType_Unique:
                    if (profile.User.Partnercode != CoreWeb.Models.User.PARTNERID_GAMESPY)
                    {
                        sb.Append(profile.User.Partnercode.ToString());
                        sb.Append("@");
                        sb.Append(profile.Uniquenick);
                    }
                    else
                    {
                        sb.Append(profile.Uniquenick);
                    }
                    break;
                case ProofType.ProofType_PreAuth:
                    password = "token challenge";
                    sb.Append(request.auth_token);
                    break;
            }
            if (client_to_server)
            {
                challenges.Add(request.client_challenge);
                challenges.Add(request.server_challenge);
            }
            else
            {
                challenges.Add(request.server_challenge);
                challenges.Add(request.client_challenge);
            }
            return GetProofString(sb.ToString(), challenges, password);
        }
        private String GetProofString(String userPortion, List<String> challenges, String password)
        {
            StringBuilder sb = new StringBuilder();
            String md5String;
            using (MD5 md5Hash = MD5.Create())
            {
                StringBuilder sBuilder = new StringBuilder();
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                md5String = sBuilder.ToString();
            }

            sb.Append(md5String);
            sb.Append(PROOF_BIG_SPACE);
            sb.Append(userPortion);
            sb.Append(challenges[0]);
            sb.Append(challenges[1]);
            sb.Append(md5String);

            using (MD5 md5Hash = MD5.Create())
            {
                StringBuilder sBuilder = new StringBuilder();
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                md5String = sBuilder.ToString();
            }
            return md5String;
        }
        private async Task<String> generateSessionKey(Profile profile)
        {
            Session model = new Session();
            model.profile = profile;
            return (await sessionRepository.Create(model)).sessionKey;
        }
    }
}
