using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Filters;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    [HasPrivileges(Privileges.TenancyRead)]
    [DefaultResponseOnException(typeof(Exception))]
    public class TenancyObjectsReportsController : RegistryBaseController
    {
        private readonly TenancyObjectsReportService reportService;
        private readonly SecurityService securityService;
        private const string odtMime = "application/vnd.oasis.opendocument.text";
        private const string odsMime = "application/vnd.oasis.opendocument.spreadsheet";

        public TenancyObjectsReportsController(TenancyObjectsReportService reportService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.securityService = securityService;
        }

        public IActionResult Index()
        {
            var vm = reportService.GetViewModel();
            return View("Index", vm);
        }

        public IActionResult GetTenancyStatistic(TenancyStatisticModalFilter modalFilter)
        {
            byte[] file;
            if (modalFilter.IdReportType == 0)
            {
                file = reportService.GetTenancyStatistic(modalFilter);
                return File(file, odsMime, "Статистика");
            }
            file = reportService.GetTenancyStatisticForCoMSReporter(modalFilter);
            return File(file, odtMime, "Статистика для МКУ ЦПМУ.odt");
        }

        public IActionResult GetTenancyOrder(TenancyOrderModalFilter modalFilter)
        {
            var file = reportService.GetTenancyOrder(modalFilter);
            return File(file, odtMime, "Распоряжения на найм жилья.odt");
        }

        public IActionResult GetTenancyNotifiesList(DateTime? dateFrom, DateTime? dateTo)
        {
            var file = reportService.GetTenancyNotifiesList(dateFrom, dateTo);
            return File(file, odsMime, "Уведомление по счетчикам (пер).ods");
        }

        public IActionResult GetPayment()
        {
            var file = reportService.GetPayment();
            return File(file, odsMime, string.Format("Плата за найм на {0}.ods", DateTime.Now.ToString("dd.MM.yyyy")));
        }
    }
}