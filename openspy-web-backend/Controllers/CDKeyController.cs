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
        public GameLookup gameLookup;
        public ProfileLookup profileLookup;
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
        private IRepository<Profile, ProfileLookup> profileRepository;
        private IRepository<Game, GameLookup> gameRepository;
        private CdKeyRepository cdkeyRepository;
        public CDKeyController(IRepository<Profile, ProfileLookup> profileRepository, IRepository<CdKey, CdKeyLookup> cdkeyRepository, IRepository<Game, GameLookup> gameRepository)
        {
            this.profileRepository = profileRepository;
            this.cdkeyRepository = (CdKeyRepository)cdkeyRepository;
            this.gameRepository = gameRepository;
        }
        [HttpPost("AssociateCDKeyToProfile")]
        public async Task<CDKeySuccessResponse> AssociateCDKeyToProfile([FromBody] CDKeyAssociateRequest request)
        {
            var profile = (await profileRepository.Lookup(request.profileLookup)).FirstOrDefault();
            if (profile == null) throw new NoSuchUserException();

            var game = (await gameRepository.Lookup(request.gameLookup)).FirstOrDefault();
            if (game == null) throw new ArgumentException();

            var cdkeyLookup = new CdKeyLookup();
            cdkeyLookup.Cdkey = request.cdkey;
            cdkeyLookup.Gameid = game.Id;

            var resp = new CDKeySuccessResponse();

            resp.success = await cdkeyRepository.AssociateCDKeyToProfile(cdkeyLookup, profile);
            return resp;
        }
        [HttpPost("GetProfileByCDKey")]
        public async Task<Profile> GetProfileByCDKey([FromBody] GetProfileByCDKeyRequest request)
        {
            var game = (await gameRepository.Lookup(request.gameLookup)).FirstOrDefault();
            if (game == null) throw new ArgumentException();

            var cdkeyLookup = new CdKeyLookup();
            cdkeyLookup.Gameid = game.Id;
            cdkeyLookup.Cdkey = request.cdkey;

            return await cdkeyRepository.LookupProfileFromCDKey(cdkeyLookup);
        }

        [HttpPost("GetCDKeyByProfile")]
        public async Task<CdKey> GetCDKeyByProfile([FromBody] GetProfileByCDKeyRequest request)
        {
            var game = (await gameRepository.Lookup(request.gameLookup)).FirstOrDefault();
            if (game == null) throw new ArgumentException();

            var cdkeyLookup = new CdKeyLookup();
            cdkeyLookup.Gameid = game.Id;
            cdkeyLookup.profileLookup = request.profileLookup;

            return await cdkeyRepository.LookupCDKeyFromProfile(cdkeyLookup);
        }

        [HttpPost("TestCDKeyValid")]
        public async Task<CDKeySuccessResponse> TestCDKeyValid([FromBody] GetProfileByCDKeyRequest request)
        {
            var resp = new CDKeySuccessResponse();

            var game = (await gameRepository.Lookup(request.gameLookup)).FirstOrDefault();
            if (game == null) throw new ArgumentException();

            var lookupRequest = new CdKeyLookup();
            lookupRequest.Cdkey = request.cdkey;
            lookupRequest.Gameid = game.Id;

            var failIfNotFound = await cdkeyRepository.LookupFailItNotFound(lookupRequest);
            if(failIfNotFound == false)
            {
                resp.success = true;
                return resp;
            }

            var keys = await cdkeyRepository.Lookup(lookupRequest);

            resp.success = keys.Count() > 0;
            return resp;
        }
    }
}
