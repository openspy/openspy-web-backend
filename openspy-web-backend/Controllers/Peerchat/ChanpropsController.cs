using CoreWeb.Controllers.generic;
using CoreWeb.Models;
using CoreWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Controllers.Peerchat
{
    [Route("v1/[controller]")]
    [Authorize(Policy = "Persist")]
    public class ChanpropsController : ModelController<ChanpropsRecord, ChanpropsLookup>
    {
        private ChanpropsRepository chanpropsRepository;
        public ChanpropsController(IRepository<ChanpropsRecord, ChanpropsLookup> chanpropsRepository) : base(chanpropsRepository)
        {
            this.chanpropsRepository = (ChanpropsRepository)chanpropsRepository;
        }
        [HttpPost("lookup")]
        [Authorize(Policy = "CoreService")]
        public override Task<IEnumerable<ChanpropsRecord>> Get([FromBody] ChanpropsLookup lookup) => base.Get(lookup);

        [HttpPost]
        public override Task<ChanpropsRecord> Update([FromBody]ChanpropsRecord value) => base.Update(value);

        [HttpPut]
        public override Task<ChanpropsRecord> Put([FromBody]ChanpropsRecord value) => base.Put(value);

        [HttpDelete]
        public override Task<DeleteStatus> Delete([FromBody]ChanpropsLookup value) => base.Delete(value);


        [HttpPost("GetEffectiveChanprops")]
        public async Task<ChanpropsRecord> GetEffectiveChanprops([FromBody]ChanpropsLookup value) {
            return await chanpropsRepository.GetEffectiveChanprops(value.channelmask);
        }
        [HttpPost("ApplyEffectiveChanprops")]
        public async Task<ChanpropsRecord> ApplyEffectiveChanprops([FromBody]ChanpropsLookup value) {
            return await chanpropsRepository.ApplyEffectiveChanprops(value.channelmask);
        }
    }
}
