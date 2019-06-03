using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.ViewComponents
{
    public class OwnerPersonComponent : ViewComponent
    {
        public OwnerPersonComponent()
        {}

        public IViewComponentResult Invoke(OwnerPersons ownerPerson, int id, string action)
        {
            ViewBag.Id = id;
            ViewBag.Action = action;
            return View("OwnerPerson", ownerPerson);
        }
    }
}
