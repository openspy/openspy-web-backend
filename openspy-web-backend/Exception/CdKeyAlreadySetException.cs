using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Exception
{
    public class CdKeyAlreadySetException : IApplicationException
    {
        public CdKeyAlreadySetException() : base("cdkey", "AlreadySet")
        {
        }
    }
}
