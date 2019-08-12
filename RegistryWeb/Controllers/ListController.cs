using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    public class ListController<LDS> : Controller where LDS : IListDataService
    {
        protected readonly LDS dataService;
        protected readonly SecurityService securityService;

        public ListController(LDS dataService, SecurityService securityService)
        {
            this.dataService = dataService;
            this.securityService = securityService;
        }
    }
}