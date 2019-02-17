using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Controllers.generic;
using CoreWeb.Models;
using CoreWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using CoreWeb.Exception;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreWeb.Controllers
{
    [Authorize(Policy = "ProfileManage")]
    public class ProfileController : ModelController<Profile, ProfileLookup>
    {
        private IRepository<User, UserLookup> userRepository;
        private ProfileRepository profileRepository;
        public ProfileController(IRepository<Profile, ProfileLookup> repository, IRepository<User, UserLookup> userRepository) : base(repository)
        {
            this.userRepository = userRepository;
            this.profileRepository = (ProfileRepository)repository;
        }
        // POST api/<controller>
        [HttpPost]
        public override async Task<Profile> Update([FromBody]Profile value)
        {
            return await base.Update(value);
        }

        // POST api/<controller>
        [HttpPost("ReplaceProfile")]
        public async Task<Profile> ReplaceProfile([FromBody]Profile value)
        { 
            return await base.Update(value);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public override async Task<Profile> Put([FromBody]Profile value)
        {
            return await base.Put(value);                
        }

        [HttpDelete]
        public override async Task<DeleteStatus> Delete([FromBody] ProfileLookup lookup)
        {
            var profiles = (await profileRepository.Lookup(lookup));
            if(profiles == null || profiles.ToArray().Length <= 0)
            {
                throw new NoSuchUserException();
            }

            var profilesLookup = new ProfileLookup();
            profilesLookup.user = new UserLookup();
            profilesLookup.user.id = profiles.First().Userid;

            var userProfiles = (await profileRepository.Lookup(profilesLookup));
            if (userProfiles == null || userProfiles.ToArray().Length <= 1)
            {
                throw new CannotDeleteLastProfileException();
            }

            return await base.Delete(lookup);
        }
    }
}
