using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Exception
{
    public class BadCdKeyException : IApplicationException
    {
        public BadCdKeyException() : base("cdkey", "BadKey")
        {
        }
    }
}
