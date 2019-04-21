using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreWeb.Crypto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreWeb.Controllers
{
    public class DefaultController : Controller
    {
        [Route("/")]
        [HttpGet]
        public IActionResult DefaultRoute() {
            return Ok();
        }
    }
}