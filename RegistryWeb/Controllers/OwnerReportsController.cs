using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
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
            try
            {
                using (Process p = new Process())
                {
                    var activityManagerPath = config.GetValue<string>("ActivityManagerPath");
                    var configXml = activityManagerPath + "templates\\registry_web\\owners\\reestr.xml";
                    var destFileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", "reestr.docx");
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = activityManagerPath + "ActivityManager.exe";
                    p.StartInfo.Arguments = " config=\"" + configXml + "\" destFileName=\"" + destFileName +
                        "\" connectionString=\"Driver={MySQL ODBC 8.0 Unicode Driver};" + connString + "\"";
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    p.WaitForExit();
                    var file = System.IO.File.ReadAllBytes(destFileName);
                    return File(file, "application/vnd.oasis.opendocument.text",
                        @"Реестр жилых и (или) не жилых помещений МКД на " + DateTime.Now.ToString("dd.MM.yyyy") +".docx");
                }
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }
    }
}