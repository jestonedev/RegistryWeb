using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewComponents
{
    public class OwnerReasonComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public OwnerReasonComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(int id = 0)
        {
            ViewBag.Id = id;
            return View("OwnerReason", registryContext.OwnerReasonTypes);
        }
    }
}
