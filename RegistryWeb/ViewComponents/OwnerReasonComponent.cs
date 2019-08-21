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

        public IViewComponentResult Invoke(OwnerReason ownerReason, int iOwner, int iReason, string action)
        {
            ViewBag.IOwner = iOwner;
            ViewBag.IReason = iReason;
            ViewBag.Action = action;
            ViewBag.OwnerReasonTypes = registryContext.OwnerReasonTypes;
            return View("OwnerReason", ownerReason);
        }
    }
}