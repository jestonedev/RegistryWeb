using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace RegistryWeb.Filters.Common
{
    public class CommonResponseOnException: Attribute
    {
        protected readonly Type[] exceptions;

        public CommonResponseOnException(params Type[] exceptions)
        {
            this.exceptions = exceptions;
        }

        protected virtual bool HandleException(ExceptionContext context)
        {
            var handle = false;
            foreach (var exception in exceptions)
            {
                if (exception.IsInstanceOfType(context.Exception))
                {
                    handle = true;
                    break;
                }
            }
            return handle;
        }
    }
}
