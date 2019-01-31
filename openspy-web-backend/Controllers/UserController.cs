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
            public String password;
        };
        public class RegisterResponse
        {
            public User user;
            public Profile profile;
        };

        /// <summary>
        /// Registers a user, and creats a profile if supplied. If user already exists, and supplied password matches, profile is created if not already existing.
        /// </summary>
        /// <param name="register">Register data</param>
        /// <returns></returns>
        [HttpPost("register")]
        [Authorize(Policy = "UserRegister")]
        public async Task<RegisterResponse> PerformRegister([FromBody] RegisterRequest register)
        {
            RegisterResponse response = new RegisterResponse();

            int? profileId = null, userId = null;

            //check for email conflicts in partnercode
            UserLookup user = new UserLookup();
            user.email = register.user.Email;
            user.partnercode = register.user.Partnercode;
            User userModel = (await userRepository.Lookup(user)).FirstOrDefault();

            //if uniquenick set, check for uniquenick conflictss in namespaceid
            if (register.profile.Uniquenick != null && userModel != null)
            {
                var checkData = await profileRepository.CheckUniqueNickInUse(register.profile.Uniquenick, register.profile.Namespaceid, register.user.Partnercode);
                if (checkData.Item1)
                {
                    if (userModel.Password.CompareTo(register.password) == 0)
                    {
                        profileId = checkData.Item2;
                        userId = checkData.Item3;
                        throw new UniqueNickInUseException(profileId, userId);
                    } else
                    {
                        throw new UserExistsException(userModel); //user exist... need to throw profileid due to GP
                    }
                }
            }

            /*if (userModel != null)
            {
                if ((userId.HasValue && userId.Value != userModel.Id) || userModel.Password.CompareTo(register.password) != 0)
                {
                    throw new UserExistsException(userModel); //user exist... need to throw profileid due to GP
                }
            }*/

            //if OK, create user, and profile
            userModel = new User();
            userModel.Email = register.user.Email;
            userModel.Partnercode = register.user.Partnercode;
            userModel.Password = register.password;

            response.user = (await userRepository.Create(userModel));

            if (register.profile != null)
            {
                Profile profileModel = new Profile();
                profileModel = register.profile;

                profileModel.Userid = response.user.Id;
                profileModel = await profileRepository.Create(profileModel);

                response.profile = profileModel;
            }

            //send registration email

            return response;

        }
    }
}