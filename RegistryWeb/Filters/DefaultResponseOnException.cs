using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RegistryWeb.Filters.Common;
using System;
using System.Linq;

namespace RegistryWeb.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class DefaultResponseOnException : CommonResponseOnException, IExceptionFilter
    {

        public Type ViewResultType { get; set; }
        public object Model { get; set; }
        public string ViewName { get; set; }

        public DefaultResponseOnException(params Type[] exceptions): base(exceptions) {
        }

        private IActionResult ViewResultBuilder(Type type, object model, ExceptionContext context)
        {
            if (type == typeof(ViewResult)) return new ViewResult()
            {
                ViewName = ViewName ?? context.RouteData.Values["action"].ToString(),
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model }
            };
            if (type == typeof(ContentResult)) return new ContentResult { Content = model.ToString() };
            if (type.FindInterfaces(new System.Reflection.TypeFilter((t, o) => true), null).Any(i => i == typeof(IActionResult)))
                return (IActionResult)Activator.CreateInstance(type, model);
            return null;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception != null && !context.ExceptionHandled)
            {
                if (!HandleException(context)) return;
                if (ViewResultType == null)
                {
                    context.Result = new ViewResult()
                    {
                        ViewName = "Error",
                        ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) {
                            Model = context.Exception?.InnerException?.Message ?? context.Exception.Message
                        }
                    };
                }
                else
                    context.Result = ViewResultBuilder(ViewResultType, Model ?? context.Exception?.InnerException?.Message ?? context.Exception.Message, context);
                context.ExceptionHandled = true;
            }
        }
    }
}
