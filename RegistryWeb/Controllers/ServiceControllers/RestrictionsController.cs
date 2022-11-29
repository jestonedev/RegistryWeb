using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.RegistryObjects;
using RegistryDb.Models.Entities.RegistryObjects.Common.Restrictions;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class RestrictionsController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public RestrictionsController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeleteRestriction(int? idRestriction)
        {
            if (idRestriction == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                            !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
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
                dateStateReg = restriction.DateStateReg?.ToString("yyyy-MM-dd"),
                description = restriction.Description,
                idRestrictionType = restriction.IdRestrictionType,
                fileOriginName = restriction.FileOriginName
            });
        }

        public IActionResult DownloadFile(int idRestriction)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Restrictions\");
            var restriction = registryContext.Restrictions.Where(r => r.IdRestriction == idRestriction).AsNoTracking().FirstOrDefault();
            if (restriction == null) return Json(new { Error = -1 });
            var filePath = Path.Combine(path, restriction.FileOriginName);
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { Error = -2 });
            }
            return File(System.IO.File.ReadAllBytes(filePath), restriction.FileMimeType, restriction.FileDisplayName);
        }

        [HttpPost]
        public IActionResult SaveRestriction(Restriction restriction, Address address, IFormFile restrictionFile, bool restrictionFileRemove)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Restrictions\");
            if (restriction == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
                return Json(new { Error = -2 });
            if (restrictionFile != null && !restrictionFileRemove)
            {
                restriction.FileDisplayName = restrictionFile.FileName;
                restriction.FileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(restrictionFile.FileName).Extension;
                restriction.FileMimeType = restrictionFile.ContentType;
                var fileStream = new FileStream(Path.Combine(path, restriction.FileOriginName), FileMode.CreateNew);
                restrictionFile.OpenReadStream().CopyTo(fileStream);
                fileStream.Close();
            }
            //Создать
            if (restriction.IdRestriction == 0)
            {
                registryContext.Restrictions.Add(restriction);
                registryContext.SaveChanges();
                var id = 0;
                if (address == null)
                    return Json(new { Error = -3 });
                if (!int.TryParse(address.Id, out id))
                    return Json(new { Error = -4 });
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

                return Json(new { restriction.IdRestriction, restriction.FileOriginName });
            }
            var restrictionDb = registryContext.Restrictions.Where(r => r.IdRestriction == restriction.IdRestriction).AsNoTracking().FirstOrDefault();
            if (restrictionDb == null)
                return Json(new { Error = -5 });
            if (restrictionFileRemove)
            {
                var fileOriginName = restrictionDb.FileOriginName;
                if (!string.IsNullOrEmpty(fileOriginName))
                {
                    var filePath = Path.Combine(path, fileOriginName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            } else
            if (restrictionFile == null)
            {
                restriction.FileOriginName = restrictionDb.FileOriginName;
                restriction.FileDisplayName = restrictionDb.FileDisplayName;
                restriction.FileMimeType = restrictionDb.FileMimeType;
            }
            //Обновить            
            registryContext.Restrictions.Update(restriction);
            registryContext.SaveChanges();
            return Json(new { restriction.IdRestriction, restriction.FileOriginName });
        }

        [HttpPost]
        public IActionResult AddRestriction(AddressTypes addressType, ActionTypeEnum action)
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
            ViewBag.CanEditBaseInfo = true;

            return PartialView("Restriction", restrictionVM);
        }
    }
}