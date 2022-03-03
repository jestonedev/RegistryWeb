using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewModel;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.RegistryObjects;

namespace RegistryWeb.ViewComponents
{
    public class RestrictionsComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public RestrictionsComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(int id, AddressTypes type, ActionTypeEnum action = ActionTypeEnum.Details, bool isTable = false)
        {
            ViewBag.Id = id;
            IEnumerable<RestrictionVM> model = null;
            if (type == AddressTypes.Building)
            {
                model = GetBuildingRestrictions(id);
            }
            if (type == AddressTypes.Premise)
            {
                model = GetPremiseRestrictions(id);
            }
            ViewBag.AddressType = type;
            ViewBag.Action = action;
            ViewBag.RestrictionTypes = registryContext.RestrictionTypes;
            if (isTable)
                return View("RestrictionsTable", model);
            return View("Restrictions", model);
        }

        private IEnumerable<RestrictionVM> GetBuildingRestrictions(int idBuilding)
        {
            var owrs = registryContext.RestrictionBuildingsAssoc
                    .Include(oba => oba.RestrictionNavigation)
                    .Where(oba => oba.IdBuilding == idBuilding)
                    .Select(oba => oba.RestrictionNavigation)
                    .Include(or => or.RestrictionTypeNavigation)
                    .OrderBy(or => or.Date);
            var r =
                from owr in owrs
                select new RestrictionVM(owr, AddressTypes.Building);
            return r;
        }

        private IEnumerable<RestrictionVM> GetPremiseRestrictions(int idPremise)
        {
            var idBuilding = registryContext.Premises
                .FirstOrDefault(p => p.IdPremises == idPremise)
                ?.IdBuilding;
            var owrs_b = registryContext.RestrictionBuildingsAssoc
                .Include(oba => oba.RestrictionNavigation)
                .Where(oba => oba.IdBuilding == idBuilding)
                .Select(oba => oba.RestrictionNavigation)
                .Include(owr => owr.RestrictionTypeNavigation);
            var r1 = owrs_b.Select(o => new RestrictionVM(o, AddressTypes.Building)).ToList();
            var owrs_pr = registryContext.RestrictionPremisesAssoc
                .Include(opa => opa.RestrictionNavigation)
                .Where(opa => opa.IdPremises == idPremise)
                .Select(opa => opa.RestrictionNavigation)
                .Include(or => or.RestrictionTypeNavigation);
            var r2 = owrs_pr.Select(o => new RestrictionVM(o, AddressTypes.Premise)).ToList();
            var r = r1.Union(r2).OrderBy(or => or.Date);
            return r;
        }
    }
}
