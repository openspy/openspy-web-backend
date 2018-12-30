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
        public GameController(IRepository<Game, GameLookup> repository) : base(repository)
        {

        }
        [HttpPost("lookup")]
        [Authorize(Policy = "CoreService")]
        public new Task<IEnumerable<Game>> Get([FromBody] GameLookup lookup) => base.Get(lookup);
    }
}
