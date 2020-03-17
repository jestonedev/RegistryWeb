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

        public IViewComponentResult Invoke(int id, AddressTypes type, string action)
        {
            ViewBag.Action = action;
            IQueryable<OwnershipRight> model = null;
            if (type == AddressTypes.Building)
                model = GetBuildingOwnershipRights(id);
            if (type == AddressTypes.Premise)
                model = GetPremiseOwnershipRights(id);
            return View("OwnershipRights", model);
        }

        private IQueryable<OwnershipRight> GetBuildingOwnershipRights(int idBuilding)
        {
            return registryContext.OwnershipBuildingsAssoc
                .Include(oba => oba.OwnershipRightNavigation)
                .Where(oba => oba.IdBuilding == idBuilding)
                .Select(oba => oba.OwnershipRightNavigation)
                .Include(or => or.OwnershipRightTypeNavigation)
                .OrderBy(or => or.Date);
        }

        private IQueryable<OwnershipRight> GetPremiseOwnershipRights(int idPremise)
        {
            return registryContext.OwnershipPremisesAssoc
                .Include(opa => opa.OwnershipRightNavigation)
                .Where(opa => opa.IdPremises == idPremise)
                .Select(opa => opa.OwnershipRightNavigation)
                .Include(or => or.OwnershipRightTypeNavigation)
                .OrderBy(or => or.Date);
        }
    }
}
