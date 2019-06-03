using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.ViewComponents
{
    public class OwnerReasonAccordionComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public OwnerReasonAccordionComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(OwnerReasons ownerReason, int id, string action)
        {
            ViewBag.Id = id;
            ViewBag.Action = action;
            ViewBag.OwnerReasonTypes = registryContext.OwnerReasonTypes;
            return View("OwnerReasonAccordion", ownerReason);
        }
    }
}