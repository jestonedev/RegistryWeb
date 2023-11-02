using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RegistryWeb.Filters.Common;
using RegistryWeb.SecurityServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class HasPrivileges : Attribute, IAuthorizationFilter, IFilterFactory
    {
        public PrivilegesComparator PrivilegesComparator { get; set; } = PrivilegesComparator.And;

        private SecurityService securityService;
        private readonly Privileges[] privileges;

        public HasPrivileges(params Privileges[] privileges) {
            this.privileges = privileges;
        }

        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            securityService = (SecurityService)serviceProvider.GetService(typeof(SecurityService));
            return this;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var allowAccess = PrivilegesComparator == PrivilegesComparator.And;
            if (privileges != null && privileges.Any())
            {
                foreach (var privilege in privileges)
                {
                    if ((PrivilegesComparator == PrivilegesComparator.And && !securityService.HasPrivilege(privilege)) ||
                        (PrivilegesComparator == PrivilegesComparator.Or && securityService.HasPrivilege(privilege))
                        )
                    {
                        allowAccess = !allowAccess;
                        break;
                    }
                }
            }
            if (!allowAccess)
            {
                context.Result = new ViewResult()
                {
                    ViewName = "NotAccess",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                };
            }
        }
    }
}
