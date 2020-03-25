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
    public class SubPremisesComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public SubPremisesComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(int id, string action)
        {
            IEnumerable<SubPremiseVM> model = null;

            model = GetSubPremises(id);
            
            ViewBag.Action = action;

            return View("SubPremises", model);
        }

        private IEnumerable<SubPremiseVM> GetSubPremises(int idPremise)
        {
            var owrs = registryContext.SubPremises
                    .Include(oba => oba.IdPremisesNavigation)
                    .Include(oba => oba.IdStateNavigation)
                    .Include(oba => oba.FundsSubPremisesAssoc).ThenInclude(fpa => fpa.IdFundNavigation).ThenInclude(fh => fh.IdFundTypeNavigation)
                    .Where(oba => oba.IdPremises == idPremise)
                    //.Select(oba => oba.SubPremisesNum)
                    .OrderBy(or => or.SubPremisesNum);
            var r =
                from owr in owrs
                select new SubPremiseVM(owr);
            return r;
        }
    }
}
