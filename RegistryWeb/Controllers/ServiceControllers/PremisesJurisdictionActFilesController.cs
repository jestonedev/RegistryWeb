using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RegistryWeb.DataHelpers;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class PremisesJurisdictionActFilesController: Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public PremisesJurisdictionActFilesController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        /*[HttpPost]
        public int DeleteJurisdiction(int? idJurisdiction)
        {
            if (idJurisdiction == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return -2;
            try
            {
                var jurisdiction = registryContext.PremisesJurisdictionActFiles
                    .FirstOrDefault(op => op.IdJurisdiction == idJurisdiction);
                jurisdiction.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetJurisdiction(int? idJurisdiction)
        {
            if (idJurisdiction == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-2);
            var jurisdiction = registryContext.PremisesJurisdictionActFiles
                .Include(or => or.IdActFileTypeDocumentNavigation)
                .FirstOrDefault(op => op.IdJurisdiction == idJurisdiction);
            return Json(new {
                number = jurisdiction.Number,
                date = jurisdiction.Date.ToString("yyyy-MM-dd"),
                name = jurisdiction.Name,
                idActFileTypeDocument = jurisdiction.IdActFileTypeDocument,
                fileOriginName = jurisdiction.FileOriginName
            });
        }

        public IActionResult DownloadFile(int idJurisdiction)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Restrictions\");
            var jurisdiction = registryContext.PremisesJurisdictionActFiles.Where(r => r.IdJurisdiction == idJurisdiction).AsNoTracking().FirstOrDefault();
            if (jurisdiction == null) return Json(new { Error = -1 });
            var filePath = Path.Combine(path, jurisdiction.FileOriginName);
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { Error = -2 });
            }
            return File(System.IO.File.ReadAllBytes(filePath), jurisdiction.FileMimeType, jurisdiction.FileDisplayName);
        }

        [HttpPost]
        public IActionResult SaveJurisdiction(PremisesJurisdictionActFiles jurisdiction, Address address, IFormFile jurisdictionFile, bool jurisdictionFileRemove)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Restrictions\");
            if (jurisdiction == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
                return Json(new { Error = -2 });
            if (jurisdictionFile != null && !jurisdictionFileRemove)
            {
                jurisdiction.FileDisplayName = jurisdictionFile.FileName;
                jurisdiction.FileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(jurisdictionFile.FileName).Extension;
                jurisdiction.FileMimeType = jurisdictionFile.ContentType;
                var fileStream = new FileStream(Path.Combine(path, jurisdiction.FileOriginName), FileMode.CreateNew);
                jurisdictionFile.OpenReadStream().CopyTo(fileStream);
                fileStream.Close();
            }
            //Создать
            if (jurisdiction.IdJurisdiction == 0)
            {
                registryContext.PremisesJurisdictionActFiles.Add(jurisdiction);
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
                        IdRestriction = jurisdiction.IdJurisdiction
                    };
                    registryContext.RestrictionBuildingsAssoc.Add(rba);
                    registryContext.SaveChanges();
                }
                if (address.AddressType == AddressTypes.Premise)
                {
                    var rpa = new RestrictionPremiseAssoc()
                    {
                        IdPremises = id,
                        IdRestriction = jurisdiction.IdJurisdiction
                    };
                    registryContext.RestrictionPremisesAssoc.Add(rpa);
                    registryContext.SaveChanges();
                }

                return Json(new { jurisdiction.IdJurisdiction, jurisdiction.FileOriginName });
            }
            var jurisdictionDb = registryContext.PremisesJurisdictionActFiles.Where(r => r.IdJurisdiction == jurisdiction.IdJurisdiction).AsNoTracking().FirstOrDefault();
            if (jurisdictionDb == null)
                return Json(new { Error = -5 });
            if (jurisdictionFileRemove)
            {
                var fileOriginName = jurisdictionDb.FileOriginName;
                if (!string.IsNullOrEmpty(fileOriginName))
                {
                    var filePath = Path.Combine(path, fileOriginName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            } else
            if (jurisdictionFile == null)
            {
                jurisdiction.FileOriginName = jurisdictionDb.FileOriginName;
                jurisdiction.FileDisplayName = jurisdictionDb.FileDisplayName;
                jurisdiction.FileMimeType = jurisdictionDb.FileMimeType;
            }
            //Обновить            
            registryContext.PremisesJurisdictionActFiles.Update(jurisdiction);
            registryContext.SaveChanges();
            return Json(new { jurisdiction.IdJurisdiction, jurisdiction.FileOriginName });
        }

        [HttpPost]
        public IActionResult AddJurisdiction(AddressTypes addressType, string action)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
                return Json(-2);

            var jurisdiction = new PremisesJurisdictionActFiles { };
            var jurisdictionVM = new PremisesJurisdictionActFileVM(jurisdiction, addressType);
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.AddressType = addressType;
            ViewBag.ActTypeDocument = registryContext.ActTypeDocuments.Where(a => a.Id > 9 && a.Id < 13).ToList();  //усл изменить

            return PartialView("PremisesJurisdictionActFile", jurisdictionVM);
        }*/
    }
}