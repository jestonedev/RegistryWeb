using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RegistryWeb.Enums;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class LitigationsController: Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public LitigationsController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeleteLitigation(int? idLitigation)
        {
            if (idLitigation == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo))
                return -2;
            try
            {
                var litigation = registryContext.Litigations
                    .FirstOrDefault(op => op.IdLitigation == idLitigation);
                litigation.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetLitigation(int? idLitigation)
        {
            if (idLitigation == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-2);
            var litigation = registryContext.Litigations
                .Include(or => or.LitigationTypeNavigation)
                .FirstOrDefault(op => op.IdLitigation == idLitigation);
            return Json(new
            {
                number = litigation.Number,
                date = litigation.Date.ToString("yyyy-MM-dd"),
                description = litigation.Description,
                idRestrictionType = litigation.IdLitigationType,
                fileOriginName = litigation.FileOriginName
            });
        }

        public IActionResult DownloadFile(int idLitigation)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Litigations\");
            var litigation = registryContext.Litigations.Where(r => r.IdLitigation == idLitigation).AsNoTracking().FirstOrDefault();
            if (litigation == null) return Json(new { Error = -1 });
            var filePath = Path.Combine(path, litigation.FileOriginName);
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { Error = -2 });
            }
            return File(System.IO.File.ReadAllBytes(filePath), litigation.FileMimeType, litigation.FileDisplayName);
        }

        [HttpPost]
        public IActionResult SaveLitigation(Litigation litigation, Address address, IFormFile litigationFile, bool litigationFileRemove)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Litigations\");
            if (litigation == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo))
                return Json(new { Error = -2 });
            if (litigationFile != null && !litigationFileRemove)
            {
                litigation.FileDisplayName = litigationFile.FileName;
                litigation.FileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(litigationFile.FileName).Extension;
                litigation.FileMimeType = litigationFile.ContentType;
                var fileStream = new FileStream(Path.Combine(path, litigation.FileOriginName), FileMode.CreateNew);
                litigationFile.OpenReadStream().CopyTo(fileStream);
                fileStream.Close();
            }
            //Создать
            if (litigation.IdLitigation == 0)
            {
                registryContext.Litigations.Add(litigation);
                registryContext.SaveChanges();
                var id = 0;
                if (address == null)
                    return Json(new { Error = -3 });
                if (!int.TryParse(address.Id, out id))
                    return Json(new { Error = -4 });
                var lpa = new LitigationPremiseAssoc()
                {
                    IdPremises = id,
                    IdLitigation = litigation.IdLitigation
                };
                registryContext.LitigationPremisesAssoc.Add(lpa);
                registryContext.SaveChanges();

                return Json(new { litigation.IdLitigation, litigation.FileOriginName });
            }
            var litigaionDb = registryContext.Litigations.Where(r => r.IdLitigation == litigation.IdLitigation).AsNoTracking().FirstOrDefault();
            if (litigaionDb == null)
                return Json(new { Error = -5 });
            if (litigationFileRemove)
            {
                var fileOriginName = litigaionDb.FileOriginName;
                if (!string.IsNullOrEmpty(fileOriginName))
                {
                    var filePath = Path.Combine(path, fileOriginName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            } else
            if (litigationFile == null)
            {
                litigation.FileOriginName = litigaionDb.FileOriginName;
                litigation.FileDisplayName = litigaionDb.FileDisplayName;
                litigation.FileMimeType = litigaionDb.FileMimeType;
            }
            //Обновить            
            registryContext.Litigations.Update(litigation);
            registryContext.SaveChanges();
            return Json(new { litigation.IdLitigation, litigation.FileOriginName });
        }

        [HttpPost]
        public IActionResult AddLitigation(AddressTypes addressType, ActionTypeEnum action)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo))
                return Json(-2);

            var litigation = new Litigation { };
            var litigationVM = new LitigationVM(litigation, addressType);
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.AddressType = addressType;
            ViewBag.LitigationTypes = registryContext.LitigationTypes.ToList();
            ViewBag.CanEditExtInfo = true;

            return PartialView("Litigation", litigationVM);
        }
    }
}