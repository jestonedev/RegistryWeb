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
    public class PremisesJurisdictionActFileComponent: ViewComponent
    {
        private RegistryContext registryContext;

        public PremisesJurisdictionActFileComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(int id, AddressTypes type, string action)
        {
            ViewBag.Id = id;
            IEnumerable<PremisesJurisdictionActFileVM> model = null;
            /*if (address.AddressType == AddressTypes.Building)
            {
                model = GetBuildingOwnershipRights(id, address);
            }*/
            if (type == AddressTypes.Premise)
            {
                model = GetPremisesJurisdictionActFiles(id);
            }
            ViewBag.AddressType = type;
            ViewBag.Action = action;
            ViewBag.ActTypeDocument = registryContext.ActTypeDocuments.Where(a => a.Id > 9 && a.Id < 13);  //усл изменить
            return View("PremisesJurisdictionActFiles", model);
        }

        /*private IEnumerable<PremisesJurisdictionActFileVM> GetBuildingOwnershipRights(int idBuilding, Address address_b)
        {
            var owrs = registryContext.OwnershipBuildingsAssoc
                .Include(oba => oba.OwnershipRightNavigation)
                .Where(oba => oba.IdBuilding == idBuilding)
                .Select(oba => oba.OwnershipRightNavigation)
                .Include(or => or.OwnershipRightTypeNavigation)
                .OrderBy(or => or.Date);
            return owrs.Select(owr => new PremisesJurisdictionActFileVM(owr, address_b));
        }*/

        private IEnumerable<PremisesJurisdictionActFileVM> GetPremisesJurisdictionActFiles(int idPremise)
        {
            /*var idBuilding = registryContext.Premises
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
            var r1 = owrs_b.Select(o => new PremisesJurisdictionActFileVM(o, address_b)).ToList();*/
            /*var owrs_pr = registryContext.PremisesJurisdictionActFiles
                .Include(opa => opa.IdActFileTypeDocumentNavigation)
                .Where(opa => opa.IdPremises == idPremise)
                ;
            var r2 = owrs_pr.Select(o => new PremisesJurisdictionActFileVM(o, AddressTypes.Premise)).ToList();*/
            //var r = r1.Union(r2).OrderBy(or => or.Date);
            return null;
        }
    }
}
