using CoreWeb.Controllers.generic;
using CoreWeb.Models;
using CoreWeb.Models.EA;
using CoreWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Controllers.FESL
{
    [Route("v1/FESL/[controller]")]
    [Authorize(Policy = "FESL")]
    public class ObjectInventoryController : ModelController<ObjectInventoryItem, ObjectInventoryLookup>
    {
        private ObjectInventoryRepository gameFeatureRepository;
        public ObjectInventoryController(IRepository<ObjectInventoryItem, ObjectInventoryLookup> gameFeatureRepository) : base(gameFeatureRepository)
        {
            this.gameFeatureRepository = (ObjectInventoryRepository)gameFeatureRepository;
        }
        [HttpPost("lookup")]
        public override Task<IEnumerable<ObjectInventoryItem>> Get([FromBody] ObjectInventoryLookup lookup) => base.Get(lookup);

        [HttpPost]
        public override Task<ObjectInventoryItem> Update([FromBody]ObjectInventoryItem value) => base.Update(value);

        [HttpPut]
        public override Task<ObjectInventoryItem> Put([FromBody]ObjectInventoryItem value) => base.Put(value);

        [HttpDelete]
        public override Task<DeleteStatus> Delete([FromBody]ObjectInventoryLookup value) => base.Delete(value);
    }
}
