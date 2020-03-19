using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewModel;

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
            IEnumerable<OwnershipRightVM> model = null;
            if (type == AddressTypes.Building)
            {
                model = GetBuildingOwnershipRights(id);
            }
            if (type == AddressTypes.Premise)
            {
                model = GetPremiseOwnershipRights(id);
            }
            ViewBag.Type = type;
            ViewBag.Action = action;
            return View("OwnershipRights", model);
        }

        private IEnumerable<OwnershipRightVM> GetBuildingOwnershipRights(int idBuilding)
        {
            var owrs = registryContext.OwnershipBuildingsAssoc
                    .Include(oba => oba.OwnershipRightNavigation)
                    .Where(oba => oba.IdBuilding == idBuilding)
                    .Select(oba => oba.OwnershipRightNavigation)
                    .Include(or => or.OwnershipRightTypeNavigation)
                    .OrderBy(or => or.Date);
            var r =
                from owr in owrs
                select new OwnershipRightVM(owr, AddressTypes.Building);
            return r;
        }

        private IEnumerable<OwnershipRightVM> GetPremiseOwnershipRights(int idPremise)
        {
            var idBuilding = registryContext.Premises
                .Include(p => p.IdBuildingNavigation)
                .FirstOrDefault(p => p.IdPremises == idPremise)
                .IdBuildingNavigation
                .IdBuilding;
            var owrs_b = registryContext.OwnershipBuildingsAssoc
                .Include(oba => oba.OwnershipRightNavigation)
                .Where(oba => oba.IdBuilding == idBuilding)
                .Select(oba => oba.OwnershipRightNavigation)
                .Include(owr => owr.OwnershipRightTypeNavigation);
            var r1 = owrs_b.Select(o => new OwnershipRightVM(o, AddressTypes.Building)).ToList();
            var owrs_pr = registryContext.OwnershipPremisesAssoc
                .Include(opa => opa.OwnershipRightNavigation)
                .Where(opa => opa.IdPremises == idPremise)
                .Select(opa => opa.OwnershipRightNavigation)
                .Include(or => or.OwnershipRightTypeNavigation);
            var r2 = owrs_pr.Select(o => new OwnershipRightVM(o, AddressTypes.Premise)).ToList();
            var r = r1.Union(r2).OrderBy(or => or.Date);
            return r;
        }
    }
}
