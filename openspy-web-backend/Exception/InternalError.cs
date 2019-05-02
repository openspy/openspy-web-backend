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
                var sendException = exception.InnerException ?? exception;
                extraData["message"] = sendException.Message;
                extraData["stackTrace"] = sendException.StackTrace;
            }
        }
    }
}
