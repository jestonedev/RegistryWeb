using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.ViewComponents
{
    public class OwnerReasonComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public OwnerReasonComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(OwnerReasons ownerReason, int id)
        {
            ViewBag.Id = id;
            ViewBag.OwnerReasonTypes = registryContext.OwnerReasonTypes;
            return View("OwnerReason", ownerReason);
        }
    }
}