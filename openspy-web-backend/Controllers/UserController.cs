using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Controllers.generic;
using CoreWeb.Models;
using CoreWeb.Repository;
using Microsoft.AspNetCore.Authorization;

namespace CoreWeb.Controllers
{
    [Authorize(Policy = "UserManage")]
    public class UserController : ModelController<User, UserLookup>
    {
        public UserController(IRepository<User, UserLookup> repository) : base(repository)
        {

        }
    }
}