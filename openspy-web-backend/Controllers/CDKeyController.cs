using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Models;
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
        public bool? abortIfExists;
    }
    public class CDKeySuccessResponse
    {
        public bool success;
    }
    [Authorize(Policy = "CDKeyManage")]
    [Route("v1/[controller]")]
    public class CDKeyController : Controller
    {
        [HttpPost("AssociateCDKeyToProfile")]
        public Task<CDKeySuccessResponse> AssociateCDKeyToProfile(CDKeyAssociateRequest request)
        {
            throw new NotImplementedException();
        }
        [HttpPost("GetProfileByCDKey")]
        public Task<Profile> GetProfileByCDKey(GetProfileByCDKeyRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("TestCDKeyValid")]
        public Task<CDKeySuccessResponse> TestCDKeyValid(GetProfileByCDKeyRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
