using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RegistryDb.Models;
using RegistryDb.Models.Entities.Claims;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class ClaimFilesController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public ClaimFilesController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeleteClaimFile(int? idFile)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Claims\");
            if (idFile == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return -2;
            try
            {
                var file = registryContext.ClaimFiles
                    .FirstOrDefault(op => op.IdFile == idFile);

                var fileOriginName = file.FileName;
                if (!string.IsNullOrEmpty(fileOriginName))
                {
                    var filePath = Path.Combine(path, fileOriginName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                registryContext.ClaimFiles.Remove(file);
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetClaimFile(int? idFile)
        {
            if (idFile == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return Json(-2);
            var file = registryContext.ClaimFiles
                .FirstOrDefault(op => op.IdFile == idFile);
            return Json(new {
                description = file.Description,
                fileOriginName = file.FileName
            });
        }

        public IActionResult DownloadFile(int idFile)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Claims\");
            var file = registryContext.ClaimFiles.Where(r => r.IdFile == idFile).AsNoTracking().FirstOrDefault();
            if (file == null) return Json(new { Error = -1 });
            var filePath = Path.Combine(path, file.FileName);
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { Error = -2 });
            }
            return File(System.IO.File.ReadAllBytes(filePath), file.MimeType ?? "application/octet-stream", file.DisplayName);
        }

        [HttpPost]
        public IActionResult SaveClaimFile(ClaimFile file, IFormFile attachmentFile, bool attachmentFileRemove)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Claims\");
            if (file == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return Json(new { Error = -2 });
            if (attachmentFile != null && !attachmentFileRemove)
            {
                file.DisplayName = attachmentFile.FileName;
                file.FileName = Guid.NewGuid().ToString() + "." + new FileInfo(attachmentFile.FileName).Extension;
                file.MimeType = attachmentFile.ContentType;
                var fileStream = new FileStream(Path.Combine(path, file.FileName), FileMode.CreateNew);
                attachmentFile.OpenReadStream().CopyTo(fileStream);
                fileStream.Close();
            }
            //Создать
            if (file.IdFile == 0)
            {
                registryContext.ClaimFiles.Add(file);
                registryContext.SaveChanges();
                return Json(new { file.IdFile, file.FileName });
            }
            var fileDb = registryContext.ClaimFiles.Where(r => r.IdFile == file.IdFile).AsNoTracking().FirstOrDefault();
            if (fileDb == null)
                return Json(new { Error = -5 });
            if (attachmentFileRemove)
            {
                var fileOriginName = fileDb.FileName;
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
                file.FileName = fileDb.FileName;
                file.DisplayName = fileDb.DisplayName;
                file.MimeType = fileDb.MimeType;
            }
            //Обновить            
            registryContext.ClaimFiles.Update(file);
            registryContext.SaveChanges();
            return Json(new { file.IdFile, file.FileName });
        }
    }
}