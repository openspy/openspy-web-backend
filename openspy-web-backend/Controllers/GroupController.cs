using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Controllers.generic;
using CoreWeb.Models;
using CoreWeb.Repository;
using Microsoft.AspNetCore.Authorization;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreWeb.Controllers
{
    [Authorize(Policy = "GroupManage")]
    public class GroupController : ModelController<Group, GroupLookup>
    {
        public GroupController(IRepository<Group, GroupLookup> repository) : base(repository)
        {

        }
        [HttpPost("lookup")]
        [Authorize(Policy = "CoreService")]
        public override Task<IEnumerable<Group>> Get([FromBody] GroupLookup lookup) => base.Get(lookup);

        [HttpPost]
        [Authorize(Policy = "GroupManage")]
        public override Task<Group> Update([FromBody]Group value) => base.Update(value);

        [HttpPut]
        [Authorize(Policy = "GroupManage")]
        public override Task<Group> Put([FromBody]Group value) => base.Put(value);

        [HttpDelete]
        [Authorize(Policy = "GroupManage")]
        public override Task<bool> Delete([FromBody]GroupLookup value) => base.Delete(value);
    }
}
