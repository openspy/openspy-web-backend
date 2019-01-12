using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Models;
using CoreWeb.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreWeb.Controllers.generic
{
    public class DeleteStatus
    {
        public bool success;
    }

    [Route("v1/[controller]")]
    [ApiController]
    public class ModelController<Mdl, Lkup> : Controller
    {
        private readonly IRepository<Mdl, Lkup> repository;
        public ModelController(IRepository<Mdl, Lkup> repository) {
            this.repository = repository;
        }

        // GET: api/<controller>
        [HttpPost("lookup")]
        public virtual Task<IEnumerable<Mdl>> Get([FromBody] Lkup lookup)
        {
            return repository.Lookup(lookup);
        }

        // POST api/<controller>
        [HttpPost]
        public virtual Task<Mdl> Update([FromBody]Mdl value)
        {
            return repository.Update(value);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public virtual Task<Mdl> Put([FromBody]Mdl value)
        {
            return repository.Create(value);
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        public virtual async Task<DeleteStatus> Delete([FromBody] Lkup lookup)
        {
            DeleteStatus status = new DeleteStatus();
            status.success = await repository.Delete(lookup);
            return status;
        }
    }
}
