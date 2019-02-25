using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CoreWeb.Exception;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

public class JsonExceptionFilter : IExceptionFilter
{
    private IConfiguration configuraiton;
    private bool development;
    public JsonExceptionFilter(IConfiguration configuration)
    {
        this.configuraiton = configuraiton;
        development = configuraiton.GetValue<string>("ASPNETCORE_ENVIRONMENT").CompareTo("development") == 0;
    }
    public void OnException(ExceptionContext context)
    {
        if (context.Exception.GetType().IsSubclassOf(typeof(IApplicationException)))
        {
            IApplicationException exception = (IApplicationException)context.Exception;
            Dictionary<string, object> error = new Dictionary<string, object>();
            error["class"] = exception._class;
            error["name"] = exception._name;
            error["extra"] = exception.extraData;
            context.Result = new ObjectResult(new { error });
        } else
        {
            if(development)
            {
                context.Result = new ObjectResult(new { context.Exception.Message, context.Exception.StackTrace });
            } else
            {
                var error = "Fatal Error";
                context.Result = new ObjectResult(new { error});
            }
            
        }
    }

}