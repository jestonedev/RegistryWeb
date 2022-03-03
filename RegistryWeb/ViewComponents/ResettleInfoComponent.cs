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
    public class ResettleInfoComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public ResettleInfoComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(Address address, ActionTypeEnum action)
        {
            IEnumerable<ResettleInfoVM> model = null;
            var id = 0;
            int.TryParse(address.Id, out id);
            // Адрес передается на случае, если в будущем надо будет добавить пересеелние в здания

            if (address.AddressType == AddressTypes.Building)
            {
                model = GetBuildingResettleInfo(id, address);
            }
            if (address.AddressType == AddressTypes.Premise)
            {
                model = GetPremiseResettleInfo(id, address);
            }
            ViewBag.Address = address;
            ViewBag.Action = action;
            ViewBag.ResettleKinds = registryContext.ResettleKinds;
            ViewBag.ResettleStages = registryContext.ResettleStages.OrderBy(rs => rs.StageName);
            ViewBag.ResettleDocumentTypes = registryContext.ResettleDocumentTypes;
            ViewBag.SubPremises = registryContext.SubPremises.Where(r => r.IdPremises == id).Select(r => new {
                r.IdSubPremises,
                SubPremisesNum = string.Concat("Комната ", r.SubPremisesNum)
            });
            ViewBag.KladrStreets = registryContext.KladrStreets.ToList();
            return View("ResettleInfoList", model);
        }

        private IEnumerable<ResettleInfoVM> GetBuildingResettleInfo(int idBuilding, Address address_b)
        {
            return null;
        }

        private IEnumerable<ResettleInfoVM> GetPremiseResettleInfo(int idPremise, Address address_p)
        {
            var resettles = registryContext.ResettlePremiseAssoc
                .Include(rpa => rpa.ResettleInfoNavigation)
                .Where(rpa => rpa.IdPremises == idPremise)
                .Select(rpa => rpa.ResettleInfoNavigation)
                .Include(ri => ri.ResettleInfoTo)
                .Include(ri => ri.ResettleInfoToFact)
                .Include(ri => ri.ResettleInfoSubPremisesFrom)
                .Include(ri => ri.ResettleDocuments)
                .Include(ri => ri.ResettleKindNavigation)
                .Include(ri => ri.ResettleKindFactNavigation)
                .Include(ri => ri.ResettleStageNavigation);
            var resettleVMs = resettles.ToList().Select(ri => new ResettleInfoVM(ri, address_p, registryContext));
            return resettleVMs.OrderBy(r => r.ResettleDate);
        }
    }
}
