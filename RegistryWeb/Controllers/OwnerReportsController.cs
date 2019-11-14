using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class OwnerReportsController : Controller
    {
        private readonly IConfiguration config;
        private readonly SecurityService securityService;
        private readonly string connString;

        public OwnerReportsController(IConfiguration config, SecurityService securityService, IHttpContextAccessor httpContextAccessor)
        {
            connString = httpContextAccessor.HttpContext.User.FindFirst("connString").Value;
            this.config = config;
            this.securityService = securityService;
        }

        public IActionResult Index()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            return View();
        }

        public IActionResult Reestr()
        {
            var logStr = new StringBuilder();
            try
            {
                using (Process p = new Process())
                {
                    
                    var activityManagerPath = config.GetValue<string>("ActivityManagerPath");
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
            }
            catch (Exception e)
            {
                logStr.Append("<dl>\n<dt>Error\n<dd>" + e.Message + "\n</dl>");
                return Error(logStr.ToString());
            }
        }

        public IActionResult Error(string msg)
        {
            ViewData["TextError"] = new HtmlString(msg);
            ViewData["Controller"] = "OwnerReports";
            return View("Error");
        }
    }
}