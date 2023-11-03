﻿using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Enums;
using RegistryWeb.Filters;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    [HasPrivileges(Privileges.PrivRead)]
    [DefaultResponseOnException(typeof(Exception))]
    public class PrivatizationReportsController : SessionController<ClaimsFilter>
    {
        private readonly PrivatizationReportService reportService;
        private readonly PrivatizationReportsDataService dataService;
        private readonly SecurityService securityService;
        private const string zipMime = "application/zip";
        private const string odtMime = "application/vnd.oasis.opendocument.text";
        private const string odsMime = "application/vnd.oasis.opendocument.spreadsheet";
        private const string xlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private const string docxMime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        public PrivatizationReportsController(PrivatizationReportService reportService, PrivatizationReportsDataService dataService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.dataService = dataService;
            this.securityService = securityService;

            nameFilteredIdsDict = "filteredPrivatizationIdsDict";
            nameIds = "idPrivatization";
            nameMultimaster = "PrivatizationReports";
        }

        public IActionResult Index()
        {
            var viewModel = dataService.GetViewModel();
            return View("Index", viewModel);
        }

        public IActionResult MonthQuarterReport(PrivQuarterReportSettings settings)
        {
            var file = reportService.GetMonthQuarterReport(settings);

            var nameReport = settings.Month.HasValue ? "Месячный отчет.odt" : "Квартальный отчет.odt";
            return File(file, odtMime, nameReport);
        }

        public IActionResult CommonReport(PrivCommonReportSettings settings)
        {
            var file = reportService.GetCommonReport(settings);
            return File(file, odtMime, string.Format("{0}.odt", settings.ReportName));
        }

        public IActionResult GetContract(PrivContractReportSettings settings)
        {
            var file = reportService.GetContract(settings);
            return File(file, odtMime, string.Format(@"Договор № {0}.odt", settings.IdContract));
        }

        public IActionResult GetContractorWarrant(PrivContractorWarrantReportSettings settings)
        {
            var file = reportService.GetContractorWarrant(settings);
            return File(file, odtMime, string.Format(@"Доверенность {1} № {0}.odt", 
                settings.IdContractor, settings.WarrantType == PrivContractorWarrantTypeEnum.Realtor ? "(риелтор)" : "в УЮ"));
        }
    }
}