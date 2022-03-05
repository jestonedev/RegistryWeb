using System;
using System.Collections.Generic;
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
using RegistryDb.Models.Entities.RegistryObjects.Common.Resettle;

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
            if (!securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo))
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
                .Include(or => or.ResettleDocuments)
                .FirstOrDefault(op => op.IdResettleInfo == idResettleInfo);

            return Json(new {
                resettleDate = resettle.ResettleDate == null ? null : resettle.ResettleDate.Value.ToString("yyyy-MM-dd"),
                idResettleKind = resettle.IdResettleKind,
                idResettleKindFact = resettle.IdResettleKindFact,
                idResettleStage = resettle.IdResettleStage,
                subPremisesFrom = resettle.ResettleInfoSubPremisesFrom.Select(r => r.IdSubPremise),
                financeSource1 = resettle.FinanceSource1,
                financeSource2 = resettle.FinanceSource2,
                financeSource3 = resettle.FinanceSource3,
                financeSource4 = resettle.FinanceSource4,
                description = resettle.Description,
                documents = resettle.ResettleDocuments.Select(r => new {
                    r.IdDocument,
                    r.Number,
                    Date = r.Date.ToString("yyyy-MM-dd"),
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
        public IActionResult SaveResettle(ResettleInfo resettleInfo, Address address, List<bool> restrictionFilesRemove)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Resettles\");
            if (resettleInfo == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo))
                return Json(new { Error = -2 });
            if (resettleInfo.ResettleDocuments != null)
            {
                for(var i = 0; i < resettleInfo.ResettleDocuments.Count; i++)
                {
                    var file = HttpContext.Request.Form.Files.Where(r => r.Name == "ResettleDocumentFiles[" + i + "]").FirstOrDefault();
                    if (file == null || restrictionFilesRemove[i] == true) continue;
                    resettleInfo.ResettleDocuments[i].FileDisplayName = file.FileName;
                    resettleInfo.ResettleDocuments[i].FileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(file.FileName).Extension;
                    resettleInfo.ResettleDocuments[i].FileMimeType = file.ContentType;
                    var fileStream = new FileStream(Path.Combine(path, resettleInfo.ResettleDocuments[i].FileOriginName), FileMode.CreateNew);
                    file.OpenReadStream().CopyTo(fileStream);
                    fileStream.Close();
                }
            }
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

                return Json(new { resettleInfo.IdResettleInfo,
                    documents = resettleInfo.ResettleDocuments.Select(r => new
                    {
                        r.IdDocument,
                        r.Number,
                        Date = r.Date.ToString("yyyy-MM-dd"),
                        r.Description,
                        r.IdDocumentType,
                        r.FileOriginName,
                    })
                });
            }
            if (resettleInfo.ResettleDocuments != null)
            {
                for (var i = 0; i < resettleInfo.ResettleDocuments.Count; i++)
                {
                    var document = resettleInfo.ResettleDocuments[i];
                    var documentDb = registryContext.ResettleDocuments.Where(r => r.IdDocument == document.IdDocument).AsNoTracking().FirstOrDefault();
                    if (document.IdDocument != 0 && documentDb == null) continue;
                    var file = HttpContext.Request.Form.Files.Where(r => r.Name == "ResettleDocumentFiles[" + i + "]").FirstOrDefault();
                    if (restrictionFilesRemove[i])
                    {
                        var fileOriginName = documentDb.FileOriginName;
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
                    if (file == null && documentDb != null)
                    {
                        document.FileOriginName = documentDb.FileOriginName;
                        document.FileDisplayName = documentDb.FileDisplayName;
                        document.FileMimeType = documentDb.FileMimeType;
                    }
                }
            }
            
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
            // Обновить переселение в (плановое)
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
            // Обновить переселение в (фактическое)
            var resettlesToFact = registryContext.ResettleInfoToFact.Where(r => r.IdResettleInfo == resettleInfo.IdResettleInfo);
            foreach (var resettleTo in resettlesToFact)
            {
                if (!resettleInfo.ResettleInfoToFact.Any(ri => ri.ObjectType == resettleTo.ObjectType && ri.IdObject == resettleTo.IdObject))
                {
                    resettleTo.Deleted = 1;
                }
            }
            var resettlesToFactList = resettlesToFact.ToList();
            foreach (var resettleTo in resettleInfo.ResettleInfoToFact)
            {
                resettleTo.IdResettleInfo = resettleInfo.IdResettleInfo;
                if (!resettlesToFactList.Any(ri => ri.ObjectType == resettleTo.ObjectType && ri.IdObject == resettleTo.IdObject))
                {
                    registryContext.ResettleInfoToFact.Add(resettleTo);
                }
            }
            // Обновить документы
            var documents = registryContext.ResettleDocuments.Where(r => r.IdResettleInfo == resettleInfo.IdResettleInfo);
            foreach (var documentDb in documents)
            {
                var document = resettleInfo.ResettleDocuments.Where(ri => ri.IdDocument == documentDb.IdDocument).FirstOrDefault();
                if (document == null)
                {
                    documentDb.Deleted = 1;
                } else
                {
                    documentDb.Number = document.Number;
                    documentDb.Date = document.Date;
                    documentDb.Description = document.Description;
                    documentDb.IdDocumentType = document.IdDocumentType;
                    documentDb.FileDisplayName = document.FileDisplayName;
                    documentDb.FileMimeType = document.FileMimeType;
                    documentDb.FileOriginName = document.FileOriginName;
                }
            }
            var documentsList = documents.ToList();
            foreach (var document in resettleInfo.ResettleDocuments)
            {
                document.IdResettleInfo = resettleInfo.IdResettleInfo;
                if (!documentsList.Any(ri => ri.IdDocument == document.IdDocument))
                {
                    registryContext.ResettleDocuments.Add(document);
                }
            }
            // Обновить основную информацию
            resettleInfo.ResettleInfoSubPremisesFrom = null;
            resettleInfo.ResettleInfoTo = null;
            resettleInfo.ResettleInfoToFact = null;
            resettleInfo.ResettleDocuments = null;
            registryContext.ResettleInfos.Update(resettleInfo);
            registryContext.SaveChanges();
            return Json(new
            {
                resettleInfo.IdResettleInfo,
                documents = resettleInfo.ResettleDocuments?.Where(r => r.Deleted == 0).Select(r => new
                            {
                                r.IdDocument,
                                r.Number,
                                Date = r.Date.ToString("yyyy-MM-dd"),
                                r.Description,
                                r.IdDocumentType,
                                r.FileOriginName,
                            })
            });
        }

        [HttpPost]
        public IActionResult AddResettle(Address address, ActionTypeEnum action)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo))
                return Json(-2);
            var id = 0;
            if (address == null)
                return Json(new { Error = -3 });
            if (!int.TryParse(address.Id, out id))
                return Json(new { Error = -4 });

            ResettleInfo resettleInfo = new ResettleInfo { };
            var resettleInfoVM = new ResettleInfoVM(resettleInfo, new Address { AddressType = address.AddressType }, registryContext);
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.Address = address;
            ViewBag.ResettleKinds = registryContext.ResettleKinds;
            ViewBag.ResettleStages = registryContext.ResettleStages.OrderBy(rs => rs.StageName);
            ViewBag.ResettleDocumentTypes = registryContext.ResettleDocumentTypes;
            ViewBag.SubPremises = registryContext.SubPremises.Where(r => r.IdPremises == id).Select(r => new {
                r.IdSubPremises,
                SubPremisesNum = string.Concat("Комната ", r.SubPremisesNum)
            });
            ViewBag.KladrStreets = registryContext.KladrStreets;
            ViewBag.CanEditExtInfo = true;

            return PartialView("ResettleInfo", resettleInfoVM);
        }

        [HttpPost]
        public IActionResult AddResettleDocument(Address address, ActionTypeEnum action)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo))
                return Json(-2);
            if (address == null)
                return Json(new { Error = -3 });

            var resettleDocument = new ResettleDocument { };
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.Address = address;
            ViewBag.ResettleDocumentTypes = registryContext.ResettleDocumentTypes;
            ViewBag.CanEditExtInfo = true;

            return PartialView("ResettleDocument", resettleDocument);
        }
    }
}