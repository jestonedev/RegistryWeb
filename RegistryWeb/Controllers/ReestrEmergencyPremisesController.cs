using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistryServices.ViewModel.RegistryObjects;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class ReestrEmergencyPremisesController : RegistryBaseController
    {
        private readonly ReestrEmergencyPremisesDataService dataService;
        private readonly SecurityService securityService;

        public ReestrEmergencyPremisesController(ReestrEmergencyPremisesDataService dataService, SecurityService securityService)
        {
            this.dataService = dataService;
            this.securityService = securityService;
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
            return View("~/Views/OwnerReports/ReestrEmergencyPremises/Index.cshtml", dataService.GetViewModel(
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
                var file = dataService.GetFileReestr();
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
            var countInhabitant = reestr.Sum(r => r.CountPersons);
            var totalAreaSum = reestr.Sum(r => r.TotalArea);
            var livingAreaSum = reestr.Sum(r => r.LivingArea);
            totalAreaSum = Math.Round(totalAreaSum, 2);
            livingAreaSum = Math.Round(livingAreaSum, 2);
            return Json(new { date, countMKD, countTenancy, countOwner, countInhabitant, totalAreaSum, livingAreaSum });
        }
    }
}