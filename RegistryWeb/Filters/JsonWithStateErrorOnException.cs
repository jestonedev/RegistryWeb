using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RegistryWeb.Filters.Common;
using System;
using System.Linq;

namespace RegistryWeb.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class JsonWithStateErrorOnException : CommonResponseOnException, IExceptionFilter
    {
        public string State { get; set; }

        public JsonWithStateErrorOnException(params Type[] exceptions) : base(exceptions)
        {
        }

        public void OnException(ExceptionContext context)
        {
            var model = new
            {
                State = State ?? "Error",
                Error = context.Exception?.InnerException?.Message ?? context.Exception.Message
            };

            if (context.Exception != null && !context.ExceptionHandled)
            {
                if (!HandleException(context)) return;
                context.Result = new JsonResult(model);
                context.ExceptionHandled = true;
            }
        }
    }
}
