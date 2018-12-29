using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CoreWeb.Exception;
using System.Collections.Generic;

public class JsonExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception.GetType().IsSubclassOf(typeof(IApplicationException)))
        {
            IApplicationException exception = (IApplicationException)context.Exception;
            Dictionary<string, string> error = new Dictionary<string, string>();
            error["class"] = exception._class;
            error["name"] = exception._name;
            context.Result = new ObjectResult(new { error });
        } else
        {
            context.Result = new ObjectResult(new { context.Exception.Message, context.Exception.StackTrace });
        }
    }

}