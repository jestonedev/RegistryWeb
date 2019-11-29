using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class OwnerReportsController : Controller
    {
        private readonly string connString;
        private readonly string activityManagerPath;
        private readonly SecurityService securityService;

        public OwnerReportsController(IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService)
        {
            connString = httpContextAccessor.HttpContext.User.FindFirst("connString").Value;
            activityManagerPath = config.GetValue<string>("ActivityManagerPath");
            this.securityService = securityService;
        }

        public IActionResult Index()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            return View();
        }

        public IActionResult Reestr(string thisController = "OwnerReports")
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            var logStr = new StringBuilder();
            try
            {                
                var p = new Process();
                var configXml = activityManagerPath + "templates\\registry_web\\owners\\reestr.xml";
                var destFileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", "reestr_" + Guid.NewGuid().ToString() + ".docx");
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = activityManagerPath + "ActivityManager.exe";
                p.StartInfo.Arguments = " config=\"" + configXml + "\" destFileName=\"" + destFileName +
                    "\" connectionString=\"Driver={" + securityService.PersonalSetting.SqlDriver.Trim() + "};" +
                    connString + "\"";
                logStr.Append("<dl>\n<dt>Arguments\n<dd>" + p.StartInfo.Arguments + "\n");
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();
                var file = System.IO.File.ReadAllBytes(destFileName);
                System.IO.File.Delete(destFileName);
                return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    @"Реестр жилых и (или) не жилых помещений МКД на " + DateTime.Now.ToString("dd.MM.yyyy") +".docx");
            }
            catch (Exception ex)
            {
                logStr.Append("<dl>\n<dt>Error\n<dd>" + ex.Message + "\n</dl>");
                return Error(logStr.ToString(), thisController);
            }
        }

        public IActionResult Error(string msg, string controller)
        {
            ViewData["TextError"] = new HtmlString(msg);
            ViewData["Controller"] = controller;
            return View("Error");
        }
    }
}