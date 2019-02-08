using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Exception
{
    public class NickInUseException : IApplicationException
    {
        public NickInUseException() : base("profile", "NickInUse")
        {
        }
    }
}
