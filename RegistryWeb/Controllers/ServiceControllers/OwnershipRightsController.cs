using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class OwnershipRightsController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public OwnershipRightsController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeleteOwnershipRight(int? idOwnershipRight)
        {
            if (idOwnershipRight == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
                return -2;
            try
            {
                var ownershipRight = registryContext.OwnershipRights
                    .FirstOrDefault(op => op.IdOwnershipRight == idOwnershipRight);
                ownershipRight.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetOwnershipRight(int? idOwnershipRight)
        {
            if (idOwnershipRight == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-2);
            var ownershipRight = registryContext.OwnershipRights
                .Include(or => or.OwnershipRightTypeNavigation)
                .FirstOrDefault(op => op.IdOwnershipRight == idOwnershipRight);
            return Json(new {
                number = ownershipRight.Number,
                date = ownershipRight.Date.ToString("yyyy-MM-dd"),
                description = ownershipRight.Description,
                idOwnershipRightType = ownershipRight.IdOwnershipRightType,
                resettlePlanDate = ownershipRight.ResettlePlanDate.HasValue ? ownershipRight.ResettlePlanDate.Value.ToString("yyyy-MM-dd") : "",
                demolishPlanDate = ownershipRight.DemolishPlanDate.HasValue ? ownershipRight.DemolishPlanDate.Value.ToString("yyyy-MM-dd") : "",
                fileOriginName = ownershipRight.FileOriginName
            });
        }

        public IActionResult DownloadFile(int idOwnershipRight)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"OwnershipRights\");
            var ownershipRight = registryContext.OwnershipRights.Where(r => r.IdOwnershipRight == idOwnershipRight).AsNoTracking().FirstOrDefault();
            if (ownershipRight == null) return Json(new { Error = -1 });
            var filePath = Path.Combine(path, ownershipRight.FileOriginName);
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { Error = -2 });
            }
            return File(System.IO.File.ReadAllBytes(filePath), ownershipRight.FileMimeType, ownershipRight.FileDisplayName);
        }

        [HttpPost]
        public IActionResult SaveOwnershipRight(OwnershipRight ownershipRight, Address address, IFormFile ownershipRightFile, bool ownershipRightFileRemove)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"OwnershipRights\");
            if (ownershipRight == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
                return Json(new { Error = -2 });
            if (ownershipRightFile != null && !ownershipRightFileRemove)
            {
                ownershipRight.FileDisplayName = ownershipRightFile.FileName;
                ownershipRight.FileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(ownershipRightFile.FileName).Extension;
                ownershipRight.FileMimeType = ownershipRightFile.ContentType;
                var fileStream = new FileStream(Path.Combine(path, ownershipRight.FileOriginName), FileMode.CreateNew);
                ownershipRightFile.OpenReadStream().CopyTo(fileStream);
                fileStream.Close();
            }
            //Создать
            if (ownershipRight.IdOwnershipRight == 0)
            {
                registryContext.OwnershipRights.Add(ownershipRight);
                registryContext.SaveChanges();
                var id = 0;
                if (address == null)
                    return Json(new { Error = -3 });
                if (!int.TryParse(address.Id, out id))
                    return Json(new { Error = -4 });
                if (address.AddressType == AddressTypes.Building)
                {
                    var oba = new OwnershipBuildingAssoc()
                    {
                        IdBuilding = id,
                        IdOwnershipRight = ownershipRight.IdOwnershipRight
                    };
                    registryContext.OwnershipBuildingsAssoc.Add(oba);
                    registryContext.SaveChanges();
                }
                if (address.AddressType == AddressTypes.Premise)
                {
                    var opa = new OwnershipPremiseAssoc()
                    {
                        IdPremises = id,
                        IdOwnershipRight = ownershipRight.IdOwnershipRight
                    };
                    registryContext.OwnershipPremisesAssoc.Add(opa);
                    registryContext.SaveChanges();
                }
                return Json(new { ownershipRight.IdOwnershipRight, ownershipRight.FileOriginName });
            }

            var ownershipRightDb = registryContext.OwnershipRights.Where(r => r.IdOwnershipRight == ownershipRight.IdOwnershipRight).AsNoTracking().FirstOrDefault();
            if (ownershipRightDb == null)
                return Json(new { Error = -5 });
            if (ownershipRightFileRemove)
            {
                var fileOriginName = ownershipRightDb.FileOriginName;
                if (!string.IsNullOrEmpty(fileOriginName))
                {
                    var filePath = Path.Combine(path, fileOriginName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            else
            if (ownershipRightFile == null)
            {
                ownershipRight.FileOriginName = ownershipRightDb.FileOriginName;
                ownershipRight.FileDisplayName = ownershipRightDb.FileDisplayName;
                ownershipRight.FileMimeType = ownershipRightDb.FileMimeType;
            }
            //Обновить            
            registryContext.OwnershipRights.Update(ownershipRight);
            registryContext.SaveChanges();
            return Json(new { ownershipRight.IdOwnershipRight, ownershipRight.FileOriginName });
        }

        [HttpPost]
        public IActionResult AddOwnershipRight(AddressTypes addressType, string action)
        {

            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
                return Json(-2);

            var owr = new OwnershipRight { };
            var address = new Address { AddressType = addressType };
            var owrVM = new OwnershipRightVM(owr, address);
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.Address = address;
            ViewBag.OwnershipRightTypes = registryContext.OwnershipRightTypes.ToList();

            return PartialView("OwnershipRight", owrVM);
        }
    }
}