using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.ViewComponents
{
    public class OwnerPersonComponent : ViewComponent
    {
        public OwnerPersonComponent()
        {}

        public IViewComponentResult Invoke(OwnerPersons ownerPerson, int id)
        {
            ViewBag.Id = id;
            return View("OwnerPerson", ownerPerson);
        }
    }
}
