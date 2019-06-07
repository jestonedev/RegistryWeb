using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.ViewComponents
{
    public class OwnerReasonComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public OwnerReasonComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(OwnerReasons ownerReason, int id, string action)
        {
            ViewBag.Id = id;
            ViewBag.Action = action;
            ViewBag.OwnerReasonTypes = registryContext.OwnerReasonTypes;
            return View("OwnerReason", ownerReason);
        }
    }
}