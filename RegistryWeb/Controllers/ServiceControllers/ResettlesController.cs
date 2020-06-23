using System;
using System.Collections.Generic;
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
    public class ResettlesController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public ResettlesController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeleteResettle(int? idResettleInfo)
        {
            if (idResettleInfo == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.RegistryWriteExtInfo))
                return -2;
            try
            {
                var resettle = registryContext.ResettleInfos
                    .FirstOrDefault(op => op.IdResettleInfo == idResettleInfo);
                resettle.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetResettle(int? idResettleInfo)
        {
            if (idResettleInfo == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-2);
            var resettle = registryContext.ResettleInfos
                .Include(or => or.ResettleInfoSubPremisesFrom)
                .Include(or => or.ResettleInfoTo)
                .Include(or => or.ResettleDocuments)
                .FirstOrDefault(op => op.IdResettleInfo == idResettleInfo);

            return Json(new {
                resettleDate = resettle.ResettleDate == null ? null : resettle.ResettleDate.Value.ToString("yyyy-MM-dd"),
                idResettleKind = resettle.IdResettleKind,
                subPremisesFrom = resettle.ResettleInfoSubPremisesFrom.Select(r => r.IdSubPremise),
                financeSource1 = resettle.FinanceSource1,
                financeSource2 = resettle.FinanceSource2,
                financeSource3 = resettle.FinanceSource3,
                financeSource4 = resettle.FinanceSource4,
                documents = resettle.ResettleDocuments.Select(r => new {
                    r.Number,
                    r.Date,
                    r.Description,
                    r.IdDocumentType,
                    r.FileOriginName,
                })
            });
        }

        public IActionResult DownloadFile(int idDocument)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Resettles\");
            var resettleDocument = registryContext.ResettleDocuments.Where(r => r.IdDocument == idDocument).AsNoTracking().FirstOrDefault();
            if (resettleDocument == null) return Json(new { Error = -1 });
            var filePath = Path.Combine(path, resettleDocument.FileOriginName);
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { Error = -2 });
            }
            return File(System.IO.File.ReadAllBytes(filePath), resettleDocument.FileMimeType, resettleDocument.FileDisplayName);
        }

        [HttpPost]
        public IActionResult SaveResettle(ResettleInfo resettleInfo, Address address)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Resettles\");
            if (resettleInfo == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.RegistryWriteExtInfo))
                return Json(new { Error = -2 });
            /*if (restrictionFile != null && !restrictionFileRemove)
            {
                restriction.FileDisplayName = restrictionFile.FileName;
                restriction.FileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(restrictionFile.FileName).Extension;
                restriction.FileMimeType = restrictionFile.ContentType;
                var fileStream = new FileStream(Path.Combine(path, restriction.FileOriginName), FileMode.CreateNew);
                restrictionFile.OpenReadStream().CopyTo(fileStream);
                fileStream.Close();
            }*/
            //Создать
            if (resettleInfo.IdResettleInfo == 0)
            {
                if (resettleInfo.ResettleInfoSubPremisesFrom != null)
                {
                    resettleInfo.ResettleInfoSubPremisesFrom = resettleInfo.ResettleInfoSubPremisesFrom.Where(r => r.IdSubPremise != 0).ToList();
                }

                registryContext.ResettleInfos.Add(resettleInfo);
                registryContext.SaveChanges();
                var id = 0;
                if (address == null)
                    return Json(new { Error = -3 });
                if (!int.TryParse(address.Id, out id))
                    return Json(new { Error = -4 });

                var rpa = new ResettlePremiseAssoc()
                {
                    IdPremises = id,
                    IdResettleInfo = resettleInfo.IdResettleInfo
                };
                registryContext.ResettlePremiseAssoc.Add(rpa);
                registryContext.SaveChanges();

                return Json(new { resettleInfo.IdResettleInfo /*, restriction.FileOriginName */ });
            }
            /*var resettleInfoDb = registryContext.ResettleInfos.Where(r => r.IdResettleInfo == resettleInfo.IdResettleInfo).AsNoTracking().FirstOrDefault();
            if (resettleInfoDb == null)
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
            }*/
            // Обновить переселение из
            var subPremisesFrom = registryContext.ResettleInfoSubPremisesFrom.Where(r => r.IdResettleInfo == resettleInfo.IdResettleInfo);
            foreach(var subPremise in subPremisesFrom)
            {
                if (!resettleInfo.ResettleInfoSubPremisesFrom.Any(ri => ri.IdSubPremise == subPremise.IdSubPremise))
                {
                    subPremise.Deleted = 1;
                }
            }
            var subPremisesFromList = subPremisesFrom.ToList();
            foreach (var subPremise in resettleInfo.ResettleInfoSubPremisesFrom)
            {
                subPremise.IdResettleInfo = resettleInfo.IdResettleInfo;
                if (subPremise.IdSubPremise != 0 && !subPremisesFromList.Any(ri => ri.IdSubPremise == subPremise.IdSubPremise))
                {
                    registryContext.ResettleInfoSubPremisesFrom.Add(subPremise);
                }
            }
            // Обновить переселение в
            var resettlesTo = registryContext.ResettleInfoTo.Where(r => r.IdResettleInfo == resettleInfo.IdResettleInfo);
            foreach (var resettleTo in resettlesTo)
            {
                if (!resettleInfo.ResettleInfoTo.Any(ri => ri.ObjectType == resettleTo.ObjectType && ri.IdObject == resettleTo.IdObject))
                {
                    resettleTo.Deleted = 1;
                }
            }
            var resettlesToList = resettlesTo.ToList();
            foreach (var resettleTo in resettleInfo.ResettleInfoTo)
            {
                resettleTo.IdResettleInfo = resettleInfo.IdResettleInfo;
                if (!resettlesToList.Any(ri => ri.ObjectType == resettleTo.ObjectType && ri.IdObject == resettleTo.IdObject))
                {
                    registryContext.ResettleInfoTo.Add(resettleTo);
                }
            }
            // Обновить основную информацию
            resettleInfo.ResettleInfoSubPremisesFrom = null;
            resettleInfo.ResettleInfoTo = null;
            registryContext.ResettleInfos.Update(resettleInfo);
            registryContext.SaveChanges();
            return Json(new { resettleInfo.IdResettleInfo /*, restriction.FileOriginName  */});
        }

        [HttpPost]
        public IActionResult AddResettle(Address address, string action)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryWriteExtInfo))
                return Json(-2);
            var id = 0;
            if (address == null)
                return Json(new { Error = -3 });
            if (!int.TryParse(address.Id, out id))
                return Json(new { Error = -4 });

            var resettleInfo = new ResettleInfo { };
            var resettleInfoVM = new ResettleInfoVM(resettleInfo, new Address { AddressType = address.AddressType }, registryContext);
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.Address = address;
            ViewBag.ResettleKinds = registryContext.ResettleKinds;
            ViewBag.ResettleDocumentTypes = registryContext.ResettleDocumentTypes;
            ViewBag.SubPremises = registryContext.SubPremises.Where(r => r.IdPremises == id).Select(r => new {
                r.IdSubPremises,
                SubPremisesNum = string.Concat("Комната ", r.SubPremisesNum)
            });
            ViewBag.KladrStreets = registryContext.KladrStreets;
            ViewBag.CanEditExtInfo = true;

            return PartialView("ResettleInfo", resettleInfoVM);
        }

        public JsonResult GetHouses(string idStreet)
        {
            IEnumerable<Building> buildings = registryContext.Buildings.Where(r => r.IdStreet == idStreet);
            return Json(buildings);
        }

        public JsonResult GetPremises(int? idBuilding)
        {
            IEnumerable<Premise> premises = registryContext.Premises.Where(r => r.IdBuilding == idBuilding);
            return Json(premises);
        }

        public JsonResult GetSubPremises(int? idPremise)
        {
            IEnumerable<SubPremise> subPremises = registryContext.SubPremises.Where(r => r.IdPremises == idPremise);
            return Json(subPremises);
        }
    }
}