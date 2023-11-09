using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryServices.DataHelpers;
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
                return File(file, MimeTypeHelper.OdsMime, "Статистика");
            }
            file = reportService.GetTenancyStatisticForCoMSReporter(modalFilter);
            return File(file, MimeTypeHelper.OdtMime, "Статистика для МКУ ЦПМУ.odt");
        }

        public IActionResult GetTenancyOrder(TenancyOrderModalFilter modalFilter)
        {
            var file = reportService.GetTenancyOrder(modalFilter);
            return File(file, MimeTypeHelper.OdtMime, "Распоряжения на найм жилья.odt");
        }

        public IActionResult GetTenancyNotifiesList(DateTime? dateFrom, DateTime? dateTo)
        {
            var file = reportService.GetTenancyNotifiesList(dateFrom, dateTo);
            return File(file, MimeTypeHelper.OdsMime, "Уведомление по счетчикам (пер).ods");
        }

        public IActionResult GetPayment()
        {
            var file = reportService.GetPayment();
            return File(file, MimeTypeHelper.OdsMime, string.Format("Плата за найм на {0}.ods", DateTime.Now.ToString("dd.MM.yyyy")));
        }
    }
}