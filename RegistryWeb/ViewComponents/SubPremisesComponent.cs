using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.SecurityServices;
using RegistryWeb.DataHelpers;

namespace RegistryWeb.ViewComponents
{
    public class SubPremisesComponent : ViewComponent
    {
        private RegistryContext registryContext;
        private readonly SecurityService securityService;

        public SubPremisesComponent(RegistryContext registryContext, SecurityService securityService)
        {
            this.registryContext = registryContext;
            this.securityService = securityService;
        }

        public IViewComponentResult Invoke(int idPremise, string action, List<PaymentsInfo> paymentsInfo)
        {
            IEnumerable<SubPremiseVM> model = GetSubPremises(idPremise, paymentsInfo);
            ViewBag.Action = action;

            var objectStates = registryContext.ObjectStates.ToList();
            ViewBag.ObjectStatesFullList = new SelectList(objectStates, "IdState", "StateFemale");
            if (action == "Create" || action == "Edit")
            {
                objectStates = objectStates.Where(r => (
                securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && ObjectStateHelper.MunicipalIds().Contains(r.IdState) ||
                securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !ObjectStateHelper.MunicipalIds().Contains(r.IdState))).ToList();
            }
            ViewBag.ObjectStatesList = new SelectList(objectStates, "IdState", "StateFemale");

            ViewBag.FundTypesList = new SelectList(registryContext.FundTypes, "IdFundType", "FundTypeName");
            ViewBag.SecurityService = securityService;
            ViewBag.IdPremises = idPremise;
            return View("SubPremises", model);
        }

        private IEnumerable<SubPremiseVM> GetSubPremises(int idPremise, List<PaymentsInfo> paymentsInfo)
        {
            var subPremises = registryContext.SubPremises
                    .Include(oba => oba.FundsSubPremisesAssoc)
                    .ThenInclude(fpa => fpa.IdFundNavigation)
                    .Where(oba => oba.IdPremises == idPremise)
                    .OrderBy(or => or.SubPremisesNum);
            return
                from subPremise in subPremises
                select new SubPremiseVM(subPremise, paymentsInfo.Where(pi => pi.IdObject == subPremise.IdSubPremises && pi.AddresType == AddressTypes.SubPremise).FirstOrDefault());
        }
    }
}
