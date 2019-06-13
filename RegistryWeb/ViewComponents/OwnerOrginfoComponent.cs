using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.ViewComponents
{
    public class OwnerOrginfoComponent : ViewComponent
    {
        public OwnerOrginfoComponent()
        { }

        public IViewComponentResult Invoke(OwnerOrginfos ownerOrginfo, int id, string action)
        {
            ViewBag.Id = id;
            ViewBag.Action = action;
            return View("OwnerOrginfo", ownerOrginfo);
        }
    }
}
