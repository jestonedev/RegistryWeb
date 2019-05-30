using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.ViewComponents
{
    public class OwnerOrginfoComponent : ViewComponent
    {
        public OwnerOrginfoComponent()
        { }

        public IViewComponentResult Invoke(OwnerOrginfos ownerOrginfo, int id)
        {
            ViewBag.Id = id;
            return View("OwnerOrginfo", ownerOrginfo);
        }
    }
}
