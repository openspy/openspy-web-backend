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
        GroupRepository groupRepository;
        public GroupController(IRepository<Group, GroupLookup> repository) : base(repository)
        {
            groupRepository = (GroupRepository)repository;
        }
        [HttpPost("lookup")]
        [Authorize(Policy = "CoreService")]
        public override Task<IEnumerable<Group>> Get([FromBody] GroupLookup lookup) => base.Get(lookup);

        [HttpPost]
        public override Task<Group> Update([FromBody]Group value) => base.Update(value);

        [HttpPut]
        public override Task<Group> Put([FromBody]Group value) => base.Put(value);

        [HttpDelete]
        public override Task<DeleteStatus> Delete([FromBody]GroupLookup value) => base.Delete(value);

        [HttpPost("SyncToRedis")]
        public async Task SyncToRedis()
        {
            await groupRepository.SyncToRedis();
        }
    }
}
