using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Exception
{
    public class InternalErrorException : IApplicationException
    {
        public InternalErrorException(System.Exception exception = null) : base("common", "InternalError")
        {
            if(exception != null)
            {
                extraData["message"] = exception.Message;
                extraData["stackTrace"] = exception.StackTrace;
            }
        }
    }
}
