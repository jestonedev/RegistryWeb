using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.SecurityServices;
using RegistryWeb.ReportServices;
using RegistryWeb.DataServices;
using RegistryWeb.Filters;

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
        private const string odtMime = "application/vnd.oasis.opendocument.text";
        private const string odsMime = "application/vnd.oasis.opendocument.spreadsheet";
        private const string xlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

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
            return File(file, odsMime, "Реестр ЖП, подлежащих переселению из АЖФ до 1 января 2017 года, подлежащих переселению в 2019-2025 года.ods");
        }

        public IActionResult GetEmergencyJP(string JFType, string regions)
        {
            var file = reportService.GetJFReport(JFType, regions, out string NameReport);
            return File(file, odsMime, NameReport);
        }

        public IActionResult GetMunicipalBuilding(string typeReport)
        {
            var file = reportService.GetStatisticBuildingReport(typeReport);
            return File(file, odsMime, "Статистика по муниципальным жилым зданиям.ods");
        }

        public IActionResult GetMunicipalPremise(string typeReport)
        {
            var file = reportService.GetStatisticPremiseReport(typeReport);
            return File(file, odsMime, "Статистика по муниципальным жилым помещениям.ods");
        }
    }
}