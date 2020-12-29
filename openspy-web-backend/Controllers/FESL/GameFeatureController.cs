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
    public class GameFeatureController : ModelController<EntitledGameFeature, UserLookup>
    {
        private GameFeatureRepository gameFeatureRepository;
        public GameFeatureController(IRepository<EntitledGameFeature, UserLookup> gameFeatureRepository) : base(gameFeatureRepository)
        {
            this.gameFeatureRepository = (GameFeatureRepository)gameFeatureRepository;
        }
        [HttpPost("lookup")]
        public override Task<IEnumerable<EntitledGameFeature>> Get([FromBody] UserLookup lookup) => base.Get(lookup);

        [HttpPost]
        public override Task<EntitledGameFeature> Update([FromBody]EntitledGameFeature value) => base.Update(value);

        [HttpPut]
        public override Task<EntitledGameFeature> Put([FromBody]EntitledGameFeature value) => base.Put(value);

        [HttpDelete]
        public override Task<DeleteStatus> Delete([FromBody]UserLookup value) => base.Delete(value);
    }
}
