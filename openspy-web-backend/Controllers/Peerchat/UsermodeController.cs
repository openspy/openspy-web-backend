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
    public class UsermodeController : ModelController<UsermodeRecord, UsermodeLookup>
    {
        private IRepository<UsermodeRecord, UsermodeLookup> uermodeRepository;
        public UsermodeController(IRepository<UsermodeRecord, UsermodeLookup> uermodeRepository) : base(uermodeRepository)
        {
            this.uermodeRepository = uermodeRepository;
        }
        [HttpPost("lookup")]
        [Authorize(Policy = "CoreService")]
        public override Task<IEnumerable<UsermodeRecord>> Get([FromBody] UsermodeLookup lookup) => base.Get(lookup);

        [HttpPost]
        public override Task<UsermodeRecord> Update([FromBody]UsermodeRecord value) => base.Update(value);

        [HttpPut]
        public override Task<UsermodeRecord> Put([FromBody]UsermodeRecord value) => base.Put(value);

        [HttpDelete]
        public override Task<DeleteStatus> Delete([FromBody]UsermodeLookup value) => base.Delete(value);
    }
}
