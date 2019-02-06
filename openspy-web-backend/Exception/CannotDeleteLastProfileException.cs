using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Exception
{
    public class CannotDeleteLastProfileException : IApplicationException
    {
        public CannotDeleteLastProfileException() : base("profile", "CannotDeleteLastProfile")
        {
        }
    }
}
