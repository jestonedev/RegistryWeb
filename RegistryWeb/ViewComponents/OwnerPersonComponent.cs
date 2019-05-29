using Microsoft.AspNetCore.Mvc;

namespace RegistryWeb.ViewComponents
{
    public class OwnerPersonComponent : ViewComponent
    {
        public OwnerPersonComponent()
        {}

        public IViewComponentResult Invoke(int id = 0)
        {
            ViewBag.Id = id;
            return View("OwnerPerson");
        }
    }
}
