using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using RegistryWeb.Models;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class OwnerReportsController : Controller
    {
        private readonly RegistryContext registryContext;
        private readonly string connString;
        private readonly string activityManagerPath;
        private readonly SecurityService securityService;

        public OwnerReportsController(RegistryContext registryContext, IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService)
        {
            this.registryContext = registryContext;
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

        public IActionResult Reestr()
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
                return Error(logStr.ToString());
            }
        }

        [HttpPost]
        public JsonResult ReestrStatistic()
        {
            var numbers = new int[] { 1, 2, 6, 7 };
            var date = @DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            var mkd = (
                from ttt in (
                    from ggg in (
                        from oba in registryContext.OwnershipBuildingsAssoc
                        join owr in registryContext.OwnershipRights
                            on oba.IdOwnershipRight equals owr.IdOwnershipRight
                        where numbers.Contains(owr.IdOwnershipRightType)
                        orderby oba.IdBuilding ascending, owr.Date descending
                        select new { oba, owr }
                    )
                    group ggg by ggg.oba.IdBuilding into gr
                    select new
                    {
                        oba = gr.Select(g => g.oba).FirstOrDefault(),
                        owr = gr.Select(g => g.owr).FirstOrDefault()
                    }
                )
                where ttt.owr.IdOwnershipRightType == 7
                select ttt
            );
            var premiseAndSubPremiseTenancy = (
                from ttt in (
                    from tac in registryContext.TenancyActiveProcesses
                    join tba in registryContext.TenancyBuildingsAssoc
                        on tac.IdProcess equals tba.IdProcess into tbaGr
                    from tbaL in tbaGr.DefaultIfEmpty()
                    join tpa in (
                        from t in registryContext.TenancyPremisesAssoc
                        join p in registryContext.Premises
                            on t.IdPremise equals p.IdPremises
                        select new { t, p }
                        ) on tac.IdProcess equals tpa.t.IdProcess into tpaGr
                    from tpaL in tpaGr.DefaultIfEmpty()
                    join tspa in (
                        from t in registryContext.TenancySubPremisesAssoc
                        join sp in registryContext.SubPremises
                            on t.IdSubPremise equals sp.IdSubPremises
                        join p in registryContext.Premises
                            on sp.IdPremises equals p.IdPremises
                        select new { t, p }
                        ) on tac.IdProcess equals tspa.t.IdProcess into tspaGr
                    from tspaL in tspaGr.DefaultIfEmpty()
                    where tbaL != null || tpaL != null || tspaL != null
                    select new
                    {
                        tac,
                        IdBuilding = tpaL != null ? tpaL.p.IdBuilding : (tspaL != null ? tspaL.p.IdBuilding : tbaL.IdBuilding),
                        TotalArea = tpaL != null ? tpaL.p.TotalArea : (tspaL != null ? tspaL.p.TotalArea : 0),
                        LivingArea = tpaL != null ? tpaL.p.LivingArea : (tspaL != null ? tspaL.p.LivingArea : 0)
                    }
                )
                join m in mkd
                    on ttt.IdBuilding equals m.oba.IdBuilding
                select new { ttt, m }
            );
            var premiseAndSubPremiseOwner = (
                from ttt in (
                    from oap in registryContext.OwnerActiveProcesses
                    join oba in registryContext.OwnerBuildingsAssoc
                        on oap.IdProcess equals oba.IdBuilding into obaGr
                    from obaL in obaGr.DefaultIfEmpty()
                    join opa in (
                        from t in registryContext.OwnerPremisesAssoc
                        join p in registryContext.Premises
                            on t.IdPremise equals p.IdPremises
                        select new { t, p }
                        ) on oap.IdProcess equals opa.t.IdProcess into opaGr
                    from opaL in opaGr.DefaultIfEmpty()
                    join ospa in (
                        from t in registryContext.OwnerSubPremisesAssoc
                        join sp in registryContext.SubPremises
                            on t.IdSubPremise equals sp.IdSubPremises
                        join p in registryContext.Premises
                            on sp.IdPremises equals p.IdPremises
                        select new { t, p }
                        ) on oap.IdProcess equals ospa.t.IdProcess into ospaGr
                    from ospaL in ospaGr.DefaultIfEmpty()
                    where obaL != null || opaL != null || ospaL != null
                    select new
                    {
                        oap,
                        IdBuilding = opaL != null ? opaL.p.IdBuilding : (ospaL != null ? ospaL.p.IdBuilding : obaL.IdBuilding),
                        TotalArea = opaL != null ? opaL.p.TotalArea : (ospaL != null ? ospaL.p.TotalArea : 0),
                        LivingArea = opaL != null ? opaL.p.LivingArea : (ospaL != null ? ospaL.p.LivingArea : 0)
                    }
                )
                join m in mkd
                    on ttt.IdBuilding equals m.oba.IdBuilding
                select new { ttt, m }
            );
            var countMKD = mkd.Count();
            var countTenancy = premiseAndSubPremiseTenancy.Count();
            var countOwner = premiseAndSubPremiseOwner.Count();
            var countInhabitant = 0;
            var totalAreaSum = 0.0;
            var livingAreaSum = 0.0;
            foreach (var ten in premiseAndSubPremiseTenancy)
            {
                countInhabitant += ten.ttt.tac.Tenants.Split(',').Length;
                totalAreaSum += ten.ttt.TotalArea;
                livingAreaSum += ten.ttt.LivingArea;

            }
            foreach (var ten in premiseAndSubPremiseOwner)
            {
                countInhabitant += ten.ttt.oap.Owners.Split(',').Length;
                totalAreaSum += ten.ttt.TotalArea;
                livingAreaSum += ten.ttt.LivingArea;
            }
            totalAreaSum = Math.Round(totalAreaSum, 2);
            livingAreaSum = Math.Round(livingAreaSum, 2);
            return Json(new { date, countMKD, countTenancy, countOwner, countInhabitant, totalAreaSum, livingAreaSum });
        }

        public IActionResult Error(string msg)
        {
            ViewData["TextError"] = new HtmlString(msg);
            ViewData["Controller"] = "OwnerReports";
            return View("Error");
        }
    }
}