using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.ViewComponents
{
    public class OwnerComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public OwnerComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(Owner owner, int id, ActionTypeEnum action)
        {
            ViewBag.Id = id;
            ViewBag.Action = action;
            return View("Owner", owner);
        }
    }
}
