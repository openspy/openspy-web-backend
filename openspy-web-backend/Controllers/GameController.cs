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
    [Authorize(Policy = "GameManage")]
    public class GameController : ModelController<Game, GameLookup>
    {
        GameRepository gameRepository;
        public GameController(IRepository<Game, GameLookup> repository) : base(repository)
        {
            gameRepository = (GameRepository)repository;
        }
        [HttpPost("lookup")]
        [Authorize(Policy = "CoreService")]
        public override Task<IEnumerable<Game>> Get([FromBody] GameLookup lookup) => base.Get(lookup);

        [HttpPost]
        public override Task<Game> Update([FromBody]Game value) => base.Update(value);

        [HttpPut]
        public override Task<Game> Put([FromBody]Game value) => base.Put(value);

        [HttpDelete]
        public override Task<DeleteStatus> Delete([FromBody]GameLookup value) => base.Delete(value);

        [HttpPost("SyncToRedis")]
        public async Task SyncToRedis()
        {
            await gameRepository.SyncToRedis();
        }
    }
}
