using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Exception
{
    public class NoSuchUserException : IApplicationException
    {
        public NoSuchUserException() : base("common", "NoSuchUser")
        {
        }
    }
}
