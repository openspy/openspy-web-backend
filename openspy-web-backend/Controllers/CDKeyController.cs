using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Exception;
using CoreWeb.Models;
using CoreWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreWeb.Controllers
{
    public class GetProfileByCDKeyRequest
    {
        public string cdkey;
        public string cdkeyMD5Hash;
        public GameLookup gameLookup;
    }
    public class CDKeyAssociateRequest
    {
        public string cdkey;
        public ProfileLookup profileLookup;
        public GameLookup gameLookup;
    }
    public class CDKeySuccessResponse
    {
        public bool success;
    }
    [Authorize(Policy = "CDKeyManage")]
    [Route("v1/[controller]")]
    public class CDKeyController : Controller
    {
        public IRepository<Profile, ProfileLookup> profileRepository;
        public CDKeyController(IRepository<Profile, ProfileLookup> profileRepository)
        {
            this.profileRepository = profileRepository;
        }
        [HttpPost("AssociateCDKeyToProfile")]
        public Task<CDKeySuccessResponse> AssociateCDKeyToProfile([FromBody] CDKeyAssociateRequest request)
        {
            return Task.Run(() =>
            {
                var resp = new CDKeySuccessResponse();
                resp.success = true;
                return resp;
            });
        }
        [HttpPost("GetProfileByCDKey")]
        public async Task<Profile> GetProfileByCDKey([FromBody] GetProfileByCDKeyRequest request)
        {
            var lookup = new ProfileLookup();
            lookup.id = 1;
            var profile = (await profileRepository.Lookup(lookup)).FirstOrDefault();
            if (profile == null) throw new NoSuchUserException();
            return profile;
        }

        [HttpPost("TestCDKeyValid")]
        public Task<CDKeySuccessResponse> TestCDKeyValid([FromBody] GetProfileByCDKeyRequest request)
        {
            return Task.Run(() =>
            {
                var resp = new CDKeySuccessResponse();
                resp.success = true;
                return resp;
            });
        }
    }
}
