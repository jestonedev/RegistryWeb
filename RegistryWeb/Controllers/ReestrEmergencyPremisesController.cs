using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistryWeb.DataServices;
using RegistryWeb.Models;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class ReestrEmergencyPremisesController : Controller
    {
        private readonly ReestrEmergencyPremisesDataService dataService;
        private readonly SecurityService securityService;
        private readonly RegistryContext registryContext;

        public ReestrEmergencyPremisesController(RegistryContext registryContext, SecurityService securityService, ReestrEmergencyPremisesDataService dataService)
        {
            this.dataService = dataService;
            this.securityService = securityService;
            this.registryContext = registryContext;
        }

        public IActionResult Index(ReestrEmergencyPremisesVM viewModel)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        public IActionResult Reestr()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            try
            {
                var sqlDriver = securityService.PersonalSetting.SqlDriver.Trim();
                var file = dataService.GetFileReestr(sqlDriver);
                return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    @"Реестр жилых и (или) не жилых помещений МКД, признанных аварийными на " + DateTime.Now.ToString("dd.MM.yyyy") + ".docx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult ReestrStatistic()
        {            
            var mkd = dataService.GetEmergencyMKD();
            var tenancyProcessesReestr = dataService.GetTenancyProcessesReestr(mkd);
            var onwerProcessesReestr = dataService.GetOwnerProcessesReestr(mkd);
            var reestr = tenancyProcessesReestr.Union(onwerProcessesReestr).ToList();
            var date = @DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            var countMKD = mkd.Count();
            var countTenancy = tenancyProcessesReestr.Count();
            var countOwner = onwerProcessesReestr.Count();
            var countInhabitant = reestr.Sum(r => r.Persons.Split(',').Length);
            var totalAreaSum = reestr.Sum(r => r.TotalArea);
            var livingAreaSum = reestr.Sum(r => r.LivingArea);
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