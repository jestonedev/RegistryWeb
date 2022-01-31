using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    public class KumiAccountsController :  ListController<KumiAccountsDataService, KumiAccountsFilter>
    {
        public KumiAccountsController(KumiAccountsDataService dataService, SecurityService securityService)
            :base(dataService, securityService)
        {

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}