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
    public class OwnershipRightsComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public OwnershipRightsComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(Address address, ActionTypeEnum action)
        {
            IEnumerable<OwnershipRightVM> model = null;
            var id = 0;
            int.TryParse(address.Id, out id);
            if (address.AddressType == AddressTypes.Building)
            {
                model = GetBuildingOwnershipRights(id, address);
            }
            if (address.AddressType == AddressTypes.Premise)
            {
                model = GetPremiseOwnershipRights(id, address);
            }
            ViewBag.Address = address;
            ViewBag.Action = action;
            ViewBag.OwnershipRightTypes = registryContext.OwnershipRightTypes;
            return View("OwnershipRights", model);
        }

        private IEnumerable<OwnershipRightVM> GetBuildingOwnershipRights(int idBuilding, Address address_b)
        {
            var owrs = registryContext.OwnershipBuildingsAssoc
                .Include(oba => oba.OwnershipRightNavigation)
                .Where(oba => oba.IdBuilding == idBuilding)
                .Select(oba => oba.OwnershipRightNavigation)
                .Include(or => or.OwnershipRightTypeNavigation)
                .OrderBy(or => or.Date);
            return owrs.Select(owr => new OwnershipRightVM(owr, address_b));
        }

        private IEnumerable<OwnershipRightVM> GetPremiseOwnershipRights(int idPremise, Address address_p)
        {
            var idBuilding = registryContext.Premises
                .FirstOrDefault(p => p.IdPremises == idPremise)
                ?.IdBuilding;
            var owrs_b = registryContext.OwnershipBuildingsAssoc
                .Include(oba => oba.OwnershipRightNavigation)
                .Where(oba => oba.IdBuilding == idBuilding)
                .Select(oba => oba.OwnershipRightNavigation)
                .Include(owr => owr.OwnershipRightTypeNavigation);
            var address_b = new Address()
            {
                AddressType = AddressTypes.Building,
                Id = idBuilding.ToString()
            };
            var r1 = owrs_b.Select(o => new OwnershipRightVM(o, address_b)).ToList();
            var owrs_pr = registryContext.OwnershipPremisesAssoc
                .Include(opa => opa.OwnershipRightNavigation)
                .Where(opa => opa.IdPremises == idPremise)
                .Select(opa => opa.OwnershipRightNavigation)
                .Include(or => or.OwnershipRightTypeNavigation);
            var r2 = owrs_pr.Select(o => new OwnershipRightVM(o, address_p)).ToList();
            var r = r1.Union(r2).OrderBy(or => or.Date);
            return r;
        }
    }
}
