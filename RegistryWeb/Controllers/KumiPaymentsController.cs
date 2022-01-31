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
    public class KumiPaymentsController : ListController<KumiPaymentsDataService, KumiPaymentsFilter>
    {
        public KumiPaymentsController(KumiPaymentsDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}