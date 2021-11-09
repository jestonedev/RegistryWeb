using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
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
            if (!securityService.HasPrivilege(Privileges.PrivRead))
                return View("NotAccess");

            var viewModel = dataService.GetViewModel();

            return View("Index", viewModel);
        }

        public IActionResult QuarterReport(PrivQuarterReportSettings settings)
        {
            return null;
        }

        public IActionResult CommonReport(PrivCommonReportSettings settings)
        {
            return null;
        }

        public IActionResult GetContract(int idContract)
        {
            return null;
        }

        public IActionResult GetContractKumi(int idContract)
        {
            return null;
        }
    }
}