using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.Models;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;

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

        public IActionResult Index(ReestrEmergencyPremisesVM viewModel, bool isBack = false)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            if (isBack)
            {
                viewModel.OrderOptions = HttpContext.Session.Get<OrderOptions>("OrderOptions");
                viewModel.PageOptions = HttpContext.Session.Get<PageOptions>("PageOptions");
                viewModel.FilterOptions = HttpContext.Session.Get<ReestrEmergencyPremisesFilter>("FilterOptions");
            }
            else
            {
                HttpContext.Session.Remove("OrderOptions");
                HttpContext.Session.Remove("PageOptions");
                HttpContext.Session.Remove("FilterOptions");
            }
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
            var reestr = tenancyProcessesReestr.Union(onwerProcessesReestr);
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
            ViewData["Controller"] = "ReestrEmergencyPremises";
            return View("Error");
        }
    }
}