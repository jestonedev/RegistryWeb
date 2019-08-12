using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    public class OwnerReportsController : Controller
    {
        private readonly IConfiguration config;
        private readonly SecurityService securityService;

        public OwnerReportsController(IConfiguration config, SecurityService securityService)
        {
            this.config = config;
            this.securityService = securityService;
        }

        public IActionResult Index()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            return View();
        }

        public IActionResult Report1()
        {
            try
            {
                using (Process p = new Process())
                {
                    var activityManagerPath = config.GetValue<string>("ActivityManagerPath");
                    var configXml = activityManagerPath + "templates\\registry_web\\owners\\report1.xml";
                    var destFileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", "report1.odt");
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = activityManagerPath + "ActivityManager.exe";
                    p.StartInfo.Arguments = " config=\"" + configXml + "\" destFileName=\"" + destFileName + "\"";
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    p.WaitForExit();
                    var file = System.IO.File.ReadAllBytes(destFileName);
                    return File(file, "application/vnd.oasis.opendocument.text", "Отчет1.odt");
                }
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }
    }
}