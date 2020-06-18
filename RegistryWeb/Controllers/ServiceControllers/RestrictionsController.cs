using System;
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
    public class RestrictionsController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;

        public RestrictionsController(SecurityService securityService, RegistryContext registryContext)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
        }

        [HttpPost]
        public int DeleteRestriction(int? idRestriction)
        {
            if (idRestriction == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return -2;
            try
            {
                var restriction = registryContext.Restrictions
                    .FirstOrDefault(op => op.IdRestriction == idRestriction);
                restriction.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetRestriction(int? idRestriction)
        {
            if (idRestriction == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-2);
            var restriction = registryContext.Restrictions
                .Include(or => or.RestrictionTypeNavigation)
                .FirstOrDefault(op => op.IdRestriction == idRestriction);
            return Json(new {
                number = restriction.Number,
                date = restriction.Date.ToString("yyyy-MM-dd"),
                description = restriction.Description,
                idRestrictionType = restriction.IdRestrictionType,
            });
        }

        [HttpPost]
        public int SaveRestriction(Restriction restriction, Address address)
        {
            if (restriction == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return -2;
            //Создать
            if (restriction.IdRestriction == 0)
            {
                registryContext.Restrictions.Add(restriction);
                registryContext.SaveChanges();
                var id = 0;
                if (address == null)
                    return -3;
                if (!int.TryParse(address.Id, out id))
                    return -4;
                if (address.AddressType == AddressTypes.Building)
                {
                    var rba = new RestrictionBuildingAssoc()
                    {
                        IdBuilding = id,
                        IdRestriction = restriction.IdRestriction
                    };
                    registryContext.RestrictionBuildingsAssoc.Add(rba);
                    registryContext.SaveChanges();
                }
                if (address.AddressType == AddressTypes.Premise)
                {
                    var rpa = new RestrictionPremiseAssoc()
                    {
                        IdPremises = id,
                        IdRestriction = restriction.IdRestriction
                    };
                    registryContext.RestrictionPremisesAssoc.Add(rpa);
                    registryContext.SaveChanges();
                }
                return restriction.IdRestriction;
            }
            //Обновить            
            registryContext.Restrictions.Update(restriction);
            registryContext.SaveChanges();
            return 0;
        }

        [HttpPost]
        public IActionResult AddRestriction(AddressTypes addressType, string action)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
                return Json(-2);

            var restriction = new Restriction { };
            var restrictionVM = new RestrictionVM(restriction, addressType);
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.AddressType = addressType;
            ViewBag.RestrictionTypes = registryContext.RestrictionTypes.ToList();

            return PartialView("~/Views/Shared/Components/RestrictionsComponent/Restriction.cshtml", restrictionVM);
        }
    }
}