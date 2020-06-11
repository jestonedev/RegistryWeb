using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataHelpers;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class SubPremisesController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;

        public SubPremisesController(SecurityService securityService, RegistryContext registryContext)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
        }

        [HttpPost]
        public int DeleteSubPremise(int? idSubPremise)
        {
            if (idSubPremise == null)
                return -1;
            try
            {
                var subPremise = registryContext.SubPremises
                    .FirstOrDefault(op => op.IdSubPremises == idSubPremise);
                if (!((securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !ObjectStateHelper.IsMunicipal(subPremise.IdState)) ||
                    (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && ObjectStateHelper.IsMunicipal(subPremise.IdState))))
                    return -2;
                subPremise.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetSubPremise(int? idSubPremise)
        {
            if (idSubPremise == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-2);

            var subPremise = registryContext.SubPremises
                .Include(oba => oba.FundsSubPremisesAssoc)
                .ThenInclude(fpa => fpa.IdFundNavigation)
                .Where(oba => oba.IdSubPremises == idSubPremise).FirstOrDefault();
            if (subPremise == null)
                return Json(-1);
            var subPremiseVM = new SubPremiseVM(subPremise, null);
            return Json(new {
                subPremiseVM.SubPremisesNum,
                subPremiseVM.Description,
                subPremiseVM.TotalArea,
                subPremiseVM.LivingArea,
                subPremiseVM.IdState,
                subPremiseVM.IdFundType,
                subPremiseVM.CadastralNum,
                subPremiseVM.CadastralCost,
                subPremiseVM.BalanceCost,
                subPremiseVM.Account
            });
        }

        [HttpPost]
        public int SaveSubPremise(SubPremise subPremise, int? idFundType)
        {
            if (subPremise == null)
                return -1;

            if (!((securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !ObjectStateHelper.IsMunicipal(subPremise.IdState)) ||
                  (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && ObjectStateHelper.IsMunicipal(subPremise.IdState))))
                return -2;
            //Создать
            if (subPremise.IdSubPremises == 0)
            {
                if (idFundType != null)
                {
                    var fund = new FundHistory
                    {
                        IdFundType = idFundType.Value
                    };

                    var fpa = new FundSubPremiseAssoc
                    {
                        IdFundNavigation = fund,
                        IdSubPremisesNavigation = subPremise
                    };
                    subPremise.FundsSubPremisesAssoc = new List<FundSubPremiseAssoc>
                    {
                        fpa
                    };
                }

                registryContext.SubPremises.Add(subPremise);
                registryContext.SaveChanges();
                return subPremise.IdSubPremises;
            }
            //Обновить            
            registryContext.SubPremises.Update(subPremise);
            registryContext.SaveChanges();
            return 0;
        }

        [HttpPost]
        public IActionResult AddSubPremise(string action)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
                return Json(-2);

            var subPremise = new SubPremise { };
            var subPremiseVM = new SubPremiseVM(subPremise, null);
            ViewBag.SecurityService = securityService;
            ViewBag.action = action;

            var objectStates = registryContext.ObjectStates.ToList();
            if (action == "Create" || action == "Edit")
            {
                objectStates = objectStates.Where(r => (
                securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && ObjectStateHelper.MunicipalIds().Contains(r.IdState) ||
                securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !ObjectStateHelper.MunicipalIds().Contains(r.IdState))).ToList();
            }
            ViewBag.ObjectStatesList = new SelectList(objectStates, "IdState", "StateFemale");
            ViewBag.FundTypesList = new SelectList(registryContext.FundTypes, "IdFundType", "FundTypeName");

            return PartialView("~/Views/Shared/Components/SubPremisesComponent/SubPremise.cshtml", subPremiseVM);
        }
    }
}