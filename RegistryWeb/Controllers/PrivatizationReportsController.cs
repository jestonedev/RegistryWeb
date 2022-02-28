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

        public IActionResult MonthQuarterReport(PrivQuarterReportSettings settings)
        {
            if (!securityService.HasPrivilege(Privileges.PrivRead))
                return View("NotAccess");
            try
            {
                var file = reportService.GetMonthQuarterReport(settings);

                var nameReport = settings.Month.HasValue ? "Месячный отчет.odt" : "Квартальный отчет.odt";
                return File(file, odtMime, nameReport);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult CommonReport(PrivCommonReportSettings settings)
        {
            if (!securityService.HasPrivilege(Privileges.PrivRead))
                return View("NotAccess");
            try
            {
                var file = reportService.GetCommonReport(settings);
                return File(file, odtMime, string.Format("{0}.odt", settings.ReportName));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetContract(PrivContractReportSettings settings)
        {
            if (!securityService.HasPrivilege(Privileges.PrivRead))
                return View("NotAccess");
            try
            {
                var file = reportService.GetContract(settings);
                return File(file, odtMime, string.Format(@"Договор № {0}.odt", settings.IdContract));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetContractorWarrant(PrivContractorWarrantReportSettings settings)
        {
            if (!securityService.HasPrivilege(Privileges.PrivRead))
                return View("NotAccess");
            try
            {
                var file = reportService.GetContractorWarrant(settings);
                return File(file, odtMime, string.Format(@"Доверенность {1} № {0}.odt", 
                    settings.IdContractor, settings.WarrantType == PrivContractorWarrantTypeEnum.Realtor ? "(риелтор)" : "в УЮ"));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}