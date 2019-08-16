using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.ViewComponents
{
    public class OwnerComponent : ViewComponent
    {
        public OwnerComponent()
        {}

        public IViewComponentResult Invoke(IOwner owner, int id, string action)
        {
            ViewBag.Id = id;
            ViewBag.Action = action;
            return View("Owner", owner);
        }
    }
}
