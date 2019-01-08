using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Models;
using CoreWeb.Repository;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using CoreWeb.Exception;

namespace CoreWeb.Controllers.Presence
{

    public class BuddyLookupRequest
    {
        public ProfileLookup profileLookup;
        public bool? reverse;
    }

    [Route("v1/Presence/[controller]")]
    [ApiController]
    [Authorize(Policy = "Presence")]
    public class StatusController : ControllerBase
    {
        private PresenceProfileStatusRepository profileStatusRepository;
        private IRepository<Buddy, BuddyLookup> buddyRepository;
        private IRepository<Block, BuddyLookup> blockRepository;
        public StatusController(IRepository<PresenceProfileStatus, PresenceProfileLookup> profileStatusRepository, IRepository<Buddy, BuddyLookup> buddyRepository, IRepository<Block, BuddyLookup> blockRepository)
        {
            this.profileStatusRepository = (PresenceProfileStatusRepository)profileStatusRepository;
            this.buddyRepository = buddyRepository;
            this.blockRepository = blockRepository;
        }
        [HttpPost("FindBuddyStatuses")]
        public Task<IEnumerable<PresenceProfileStatus>> FindBuddyStatuses([FromBody] BuddyLookupRequest buddyLookup)
        {
            PresenceProfileLookup lookup = new PresenceProfileLookup();
            lookup.buddyLookup = true;
            lookup.profileLookup = buddyLookup.profileLookup;
            return profileStatusRepository.Lookup(lookup);
        }

        [HttpPost("FindBlockStatuses")]
        public Task<IEnumerable<PresenceProfileStatus>> FindBlockStatuses([FromBody] BuddyLookupRequest buddyLookup)
        {
            PresenceProfileLookup lookup = new PresenceProfileLookup();
            lookup.blockLookup = true;
            lookup.profileLookup = buddyLookup.profileLookup;
            return profileStatusRepository.Lookup(lookup);
        }

        [HttpPost("GetStatus")]
        public async Task<PresenceProfileStatus> GetStatus([FromBody] ProfileLookup profileLookup)
        {
            PresenceProfileLookup lookup = new PresenceProfileLookup();
            lookup.profileLookup = profileLookup;
            PresenceProfileStatus status = (await profileStatusRepository.Lookup(lookup)).FirstOrDefault();
            if (status == null) throw new NoSuchUserException();
            return status;
        }

        [HttpPost("SetStatus")]
        public Task<PresenceProfileStatus> SetStatus([FromBody] PresenceProfileStatus profileLookup)
        {
            return profileStatusRepository.Update(profileLookup);
        }
    }
}