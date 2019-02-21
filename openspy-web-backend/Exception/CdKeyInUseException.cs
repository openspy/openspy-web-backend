using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Exception
{
    public class CdKeyInUseException : IApplicationException
    {
        public CdKeyInUseException() : base("cdkey", "InUse")
        {
        }
    }
}
