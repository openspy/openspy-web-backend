using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Models;
using CoreWeb.Database;
using ServiceStack.Redis;
using CoreWeb.Repository;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreWeb.Controllers.Presence
{
    [Route("v1/Presence/[controller]")]
    [ApiController]
    [Authorize(Policy = "Presence")]
    public class ListController : Controller
    {
        private IRepository<User, UserLookup> userRepository;
        private IRepository<Profile, ProfileLookup> profileRepository;
        private BuddyRepository buddyRepository;
        private BlockRepository blockRepository;
        private PresenceProfileStatusRepository presenceProfileStatusRepository;

        public ListController(IRepository<User, UserLookup> userRepository, IRepository<Profile, ProfileLookup> profileRepository, IRepository<Buddy, BuddyLookup> buddyRepository, IRepository<Block, BuddyLookup> blockRepository, IRepository<PresenceProfileStatus, PresenceProfileLookup> presenceProfileStatusRepository)
        {
            this.userRepository = userRepository;
            this.profileRepository = profileRepository;
            this.buddyRepository = (BuddyRepository)buddyRepository;
            this.blockRepository = (BlockRepository)blockRepository;
            this.presenceProfileStatusRepository = (PresenceProfileStatusRepository)presenceProfileStatusRepository;
        }
        [HttpPut("Buddy")]
        public async Task PutBuddy([FromBody] BuddyLookup lookupData)
        {
            await buddyRepository.SendBuddyRequest(lookupData);
        }
        [HttpPut("Block")]
        public async Task PutBlock([FromBody] BuddyLookup lookupData)
        {
            var from_profile = (await profileRepository.Lookup(lookupData.SourceProfile)).First();
            var to_profile = (await profileRepository.Lookup(lookupData.TargetProfile)).First();
            Block block = new Block();
            block.FromProfileid = from_profile.Id;
            block.ToProfileid = to_profile.Id;
            await blockRepository.Create(block);
            blockRepository.SendAddEvent(from_profile, to_profile);
        }

        [HttpDelete("Buddy")]
        public async Task<bool> DeleteBuddy([FromBody] BuddyLookup lookupData)
        {
            //check if buddy is added to list
            //if not, check for request
            //if not, throw exception
            var items = await buddyRepository.Lookup(lookupData);
            if(items.Count() <= 0)
            {
                var from_profile = (await profileRepository.Lookup(lookupData.SourceProfile)).First();
                var to_profile = (await profileRepository.Lookup(lookupData.TargetProfile)).First();
                if (buddyRepository.DeleteBuddyRequest(from_profile, to_profile) && (!lookupData.silent.HasValue || (lookupData.silent.HasValue && lookupData.silent.Value)))
                {
                    //TODO: check if offline... add to redis to resend when user logs in
                    await buddyRepository.Delete(lookupData); //delete just in case
                    buddyRepository.SendDeleteEvent(from_profile, to_profile);
                    return true;
                }
                throw new ArgumentException();
            }
            return await buddyRepository.Delete(lookupData);
        }
        [HttpDelete("Block")]
        public async Task<bool> DeleteBlock([FromBody] BuddyLookup lookupData)
        {
            bool delete_status = await blockRepository.Delete(lookupData);
            if(delete_status)
            {
                var from_profile = (await profileRepository.Lookup(lookupData.SourceProfile)).First();
                var to_profile = (await profileRepository.Lookup(lookupData.TargetProfile)).First();
                blockRepository.SendDeleteEvent(from_profile, to_profile);
                
            }
            return delete_status;
        }

        [HttpPost("AuthorizeAdd")]
        public async Task CreateAuthorizeAdd([FromBody] BuddyLookup lookupData)
        {
            var from_profile = (await profileRepository.Lookup(lookupData.SourceProfile)).First();
            var to_profile = (await profileRepository.Lookup(lookupData.TargetProfile)).First();
            bool status_update = await buddyRepository.AuthorizeAdd(from_profile, to_profile);

            if (status_update)
            {
                await presenceProfileStatusRepository.SendStatusUpdate(to_profile);
                await presenceProfileStatusRepository.SendStatusUpdate(from_profile);
            }
        }

        [HttpPost("LookupBuddy")]
        public async Task<List<Profile>> GetBuddies([FromBody] BuddyLookup lookupData)
        {
            var buddies = (await buddyRepository.Lookup(lookupData)).ToList();
            List<Profile> profiles = new List<Profile>();
            for (int i = 0; i < buddies.Count; i++) {
                if(lookupData.reverseLookup.HasValue && lookupData.reverseLookup.Value)
                {
                    profiles.Add(buddies[i].FromProfile);
                } else
                {
                    profiles.Add(buddies[i].ToProfile);
                }
            }
            return profiles;
        }

        [HttpPost("LookupBlock")]
        public async Task<List<Profile>> GetBlocks([FromBody] BuddyLookup lookupData)
        {
            var blocks = (await blockRepository.Lookup(lookupData)).ToList();
            List<Profile> profiles = new List<Profile>();
            for (int i = 0; i < blocks.Count; i++)
            {
                if (lookupData.reverseLookup.HasValue && lookupData.reverseLookup.Value)
                {
                    profiles.Add(blocks[i].FromProfile);
                }
                else
                {
                    profiles.Add(blocks[i].ToProfile);
                }
            }
            return profiles;
        }

        [HttpPost("SendMessage")]
        public async Task<bool> SendMessage([FromBody] SendMessageRequest messageData)
        {
            await buddyRepository.SendMessage(messageData);
            return true;
        }
    }
}
