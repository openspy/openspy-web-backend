using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Exception
{
    public class InternalErrorException : IApplicationException
    {
        public InternalErrorException() : base("common", "InternalError")
        {
        }
    }
}
