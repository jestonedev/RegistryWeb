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
using RegistryDb.Models.Entities.RegistryObjects.Buildings;
using RegistryDb.Models.Entities.RegistryObjects.Common;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class AttachmentFilesController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public AttachmentFilesController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeleteAttachmentFile(int? idAttachment)
        {
            if (idAttachment == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles))
                return -2;
            try
            {
                var attachment = registryContext.ObjectAttachmentFiles
                    .FirstOrDefault(op => op.IdAttachment == idAttachment);
                attachment.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetAttachmentFile(int? idAttachment)
        {
            if (idAttachment == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return Json(-2);
            var attachment = registryContext.ObjectAttachmentFiles
                .FirstOrDefault(op => op.IdAttachment == idAttachment);
            return Json(new {
                description = attachment.Description,
                fileOriginName = attachment.FileOriginName
            });
        }

        public IActionResult DownloadFile(int idAttachment)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Attachments\");
            var attachment = registryContext.ObjectAttachmentFiles.Where(r => r.IdAttachment == idAttachment).AsNoTracking().FirstOrDefault();
            if (attachment == null) return Json(new { Error = -1 });
            var filePath = Path.Combine(path, attachment.FileOriginName);
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { Error = -2 });
            }
            return File(System.IO.File.ReadAllBytes(filePath), attachment.FileMimeType, attachment.FileDisplayName);
        }

        [HttpPost]
        public IActionResult SaveAttachmentFile(ObjectAttachmentFile attachment, Address address, IFormFile attachmentFile, bool attachmentFileRemove)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Attachments\");
            if (attachment == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles))
                return Json(new { Error = -2 });
            if (attachmentFile != null && !attachmentFileRemove)
            {
                attachment.FileDisplayName = attachmentFile.FileName;
                attachment.FileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(attachmentFile.FileName).Extension;
                attachment.FileMimeType = attachmentFile.ContentType;
                var fileStream = new FileStream(Path.Combine(path, attachment.FileOriginName), FileMode.CreateNew);
                attachmentFile.OpenReadStream().CopyTo(fileStream);
                fileStream.Close();
            }
            //Создать
            if (attachment.IdAttachment == 0)
            {
                registryContext.ObjectAttachmentFiles.Add(attachment);
                registryContext.SaveChanges();
                var id = 0;
                if (address == null)
                    return Json(new { Error = -3 });
                if (!int.TryParse(address.Id, out id))
                    return Json(new { Error = -4 });
                if (address.AddressType == AddressTypes.Building)
                {
                    var bafa = new BuildingAttachmentFileAssoc()
                    {
                        IdBuilding = id,
                        IdAttachment = attachment.IdAttachment
                    };
                    registryContext.BuildingAttachmentFilesAssoc.Add(bafa);
                    registryContext.SaveChanges();
                }

                return Json(new { attachment.IdAttachment, attachment.FileOriginName });
            }
            var attachmentDb = registryContext.ObjectAttachmentFiles.Where(r => r.IdAttachment == attachment.IdAttachment).AsNoTracking().FirstOrDefault();
            if (attachmentDb == null)
                return Json(new { Error = -5 });
            if (attachmentFileRemove)
            {
                var fileOriginName = attachmentDb.FileOriginName;
                if (!string.IsNullOrEmpty(fileOriginName))
                {
                    var filePath = Path.Combine(path, fileOriginName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            } else
            if (attachmentFile == null)
            {
                attachment.FileOriginName = attachmentDb.FileOriginName;
                attachment.FileDisplayName = attachmentDb.FileDisplayName;
                attachment.FileMimeType = attachmentDb.FileMimeType;
            }
            //Обновить            
            registryContext.ObjectAttachmentFiles.Update(attachment);
            registryContext.SaveChanges();
            return Json(new { attachment.IdAttachment, attachment.FileOriginName });
        }

        [HttpPost]
        public IActionResult AddAttachmentFile(AddressTypes addressType, ActionTypeEnum action)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles))
                return Json(-2);

            var attachment = new ObjectAttachmentFile { };
            var attachmentFileVM = new AttachmentFileVM(attachment, addressType);
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.AddressType = addressType;
            ViewBag.CanAttachAdditionalFiles = true;

            return PartialView("AttachmentFile", attachmentFileVM);
        }
    }
}