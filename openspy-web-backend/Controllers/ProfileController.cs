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
    [Authorize(Policy = "ProfileManage")]
    public class ProfileController : ModelController<Profile, ProfileLookup>
    {
        public ProfileController(IRepository<Profile, ProfileLookup> repository) : base(repository)
        {

        }
    }
}
