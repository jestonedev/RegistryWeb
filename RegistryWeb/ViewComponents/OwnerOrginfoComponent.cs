using Microsoft.AspNetCore.Mvc;

namespace RegistryWeb.ViewComponents
{
    public class OwnerOrginfoComponent : ViewComponent
    {
        public OwnerOrginfoComponent()
        { }

        public IViewComponentResult Invoke(int id = 0)
        {
            ViewBag.Id = id;
            return View("OwnerOrginfo");
        }
    }
}
