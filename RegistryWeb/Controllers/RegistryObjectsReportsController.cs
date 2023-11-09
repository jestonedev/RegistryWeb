using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.SecurityServices;
using RegistryWeb.ReportServices;
using RegistryWeb.DataServices;
using RegistryWeb.Filters;
using RegistryServices.DataHelpers;

namespace RegistryWeb.Controllers
{
    [Authorize]
    [HasPrivileges(Privileges.RegistryRead)]
    [DefaultResponseOnException(typeof(Exception))]
    public class RegistryObjectsReportsController: RegistryBaseController
    {
        private readonly RegistryObjectsReportService reportService;

        private readonly RegistryObjectsDataService dataService;
        private readonly SecurityService securityService;

        public RegistryObjectsReportsController(RegistryObjectsReportService reportService, RegistryObjectsDataService dataService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.dataService = dataService;
            this.securityService = securityService;
        }

        public IActionResult Index()
        {
            var viewModel = dataService.GetViewModel();           
            return View("Index", viewModel);
        }

        public IActionResult GetResettle_2019_2025()
        {
            var file = reportService.GetResettle_2019_2025();
            return File(file, MimeTypeHelper.OdsMime, "Реестр ЖП, подлежащих переселению из АЖФ до 1 января 2017 года, подлежащих переселению в 2019-2025 года.ods");
        }

        public IActionResult GetEmergencyJP(string JFType, string regions)
        {
            var file = reportService.GetJFReport(JFType, regions, out string NameReport);
            return File(file, MimeTypeHelper.OdsMime, NameReport);
        }

        public IActionResult GetMunicipalBuilding(string typeReport)
        {
            var file = reportService.GetStatisticBuildingReport(typeReport);
            return File(file, MimeTypeHelper.OdsMime, "Статистика по муниципальным жилым зданиям.ods");
        }

        public IActionResult GetMunicipalPremise(string typeReport)
        {
            var file = reportService.GetStatisticPremiseReport(typeReport);
            return File(file, MimeTypeHelper.OdsMime, "Статистика по муниципальным жилым помещениям.ods");
        }
    }
}