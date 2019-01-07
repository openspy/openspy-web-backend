using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Models;

namespace CoreWeb.Exception
{
    public class UserExistsException : IApplicationException
    {
        public UserExistsException(User user) : base("common", "UserExists")
        {
            this.extraData["userId"] = user.Id;
        }
    }
}
