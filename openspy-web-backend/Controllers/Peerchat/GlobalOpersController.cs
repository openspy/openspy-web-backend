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
    public class GlobalOpersController : ModelController<GlobalOpersRecord, GlobalOpersLookup>
    {
        private GlobalOpersRepository globalopersRepository;
        public GlobalOpersController(IRepository<GlobalOpersRecord, GlobalOpersLookup> globalopersRepository) : base(globalopersRepository)
        {
            this.globalopersRepository = (GlobalOpersRepository)globalopersRepository;
        }
        [HttpPost("lookup")]
        [Authorize(Policy = "CoreService")]
        public override Task<IEnumerable<GlobalOpersRecord>> Get([FromBody] GlobalOpersLookup lookup) => base.Get(lookup);

        [HttpPost]
        public override Task<GlobalOpersRecord> Update([FromBody]GlobalOpersRecord value) => base.Update(value);

        [HttpPut]
        public override Task<GlobalOpersRecord> Put([FromBody]GlobalOpersRecord value) => base.Put(value);

        [HttpDelete]
        public override Task<DeleteStatus> Delete([FromBody]GlobalOpersLookup value) => base.Delete(value);
    }
}
