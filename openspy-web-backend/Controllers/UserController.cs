using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Controllers.generic;
using CoreWeb.Models;
using CoreWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using CoreWeb.Exception;

namespace CoreWeb.Controllers
{
    [Authorize(Policy = "UserManage")]
    public class UserController : ModelController<User, UserLookup>
    {
        IRepository<User, UserLookup> userRepository;
        ProfileRepository profileRepository;
        public UserController(IRepository<User, UserLookup> userRepository, IRepository<Profile, ProfileLookup> profileRepository) : base(userRepository)
        {
            this.userRepository = userRepository;
            this.profileRepository = (ProfileRepository)profileRepository;
        }

        public class RegisterRequest
        {
            public Profile profile;
            public User user;

        };
        public class RegisterResponse
        {
            public User user;
            public Profile profile;
        };

        [HttpPost("register")]
        public async Task<RegisterResponse> PerformRegister([FromBody] RegisterRequest register)
        {
            RegisterResponse response = new RegisterResponse();
            //if uniquenick set, check for uniquenick conflictss in namespaceid
            if (register.profile.Uniquenick.Length != 0)
            {
                var checkData = await profileRepository.CheckUniqueNickInUse(register.profile.Uniquenick, register.profile.Namespaceid, register.user.Partnercode);
                if (checkData.Item1)
                {
                    throw new UniqueNickInUseException(checkData.Item2); //TODO: unique nick in use exception
                }
            }


            //check for email conflicts in partnercode
            UserLookup user = new UserLookup();
            user.email = register.user.Email;
            user.partnercode = register.user.Partnercode;
            User userModel = (await userRepository.Lookup(user)).First();
            if(userModel != null)
            {
                int ?profileid = null;
                if(userModel.Password.CompareTo(register.user.Password) == 0)
                {
                    profileid = register.profile.Id;
                }
                throw new UniqueNickInUseException(profileid); //user doesn't exist... need to throw profileid due to GP
            }

            //if OK, create user, and profile
            userModel = new User();
            userModel.Email = register.user.Email;
            userModel.Partnercode = register.user.Partnercode;

            response.user = (await userRepository.Create(userModel));

            Profile profileModel = new Profile();
            profileModel = register.profile;

            profileModel = await profileRepository.Create(profileModel);

            response.profile = profileModel;

            //send registration email

            return response;

        }
    }
}