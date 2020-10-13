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
    public class LitigationComponent: ViewComponent
    {
        private RegistryContext registryContext;

        public LitigationComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(int id, AddressTypes type, ActionTypeEnum action)
        {
            ViewBag.Id = id;
            IEnumerable<LitigationVM> model = null;
            if (type == AddressTypes.Building)
            {
                model = GetBuildingLitigations(id);
            }
            if (type  == AddressTypes.Premise)
            {
                model = GetPremiseLitigations(id);
            }
            ViewBag.AddressType = type;
            ViewBag.Action = action;
            ViewBag.LitigationTypes = registryContext.LitigationTypes;

            return View("Litigations", model);
        }

        private IEnumerable<LitigationVM> GetBuildingLitigations(int idBuilding)
        {
            return null;
        }

        private IEnumerable<LitigationVM> GetPremiseLitigations(int idPremise)
        {
            var litigations = registryContext.LitigationPremisesAssoc
                .Include(rpa => rpa.LitigationNavigation)
                .Where(rpa => rpa.IdPremises == idPremise)
                .Select(rpa => rpa.LitigationNavigation)
                .Include(ri => ri.LitigationTypeNavigation);
            var litigationVMs = litigations.ToList().Select(ri => new LitigationVM(ri, AddressTypes.Premise));
            return litigationVMs.OrderBy(r => r.Date);
        }
    }
}
