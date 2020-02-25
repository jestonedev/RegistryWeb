using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace RegistryWeb.ViewComponents
{
    public class OwnershipRightsComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public OwnershipRightsComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(int idBuilding, string action)
        {
            ViewBag.Action = action;
            var model = GetQueryOwnershipRights(idBuilding);
            return View("OwnershipRights", model);
        }

        private IQueryable<OwnershipRight> GetQueryOwnershipRights(int idBuilding)
        {
            return registryContext.OwnershipBuildingsAssoc
                .Include(oba => oba.OwnershipRightNavigation)
                .Where(oba => oba.IdBuilding == idBuilding)
                .Select(oba => oba.OwnershipRightNavigation)
                .Include(or => or.OwnershipRightTypeNavigation)
                .OrderBy(or => or.Date);
        }
    }
}
