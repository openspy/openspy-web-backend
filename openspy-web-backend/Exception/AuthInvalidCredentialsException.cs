using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Exception
{
    public class AuthInvalidCredentialsException : IApplicationException
    {
        public AuthInvalidCredentialsException() : base("auth", "InvalidCredentials")
        {
        }
    }
}
