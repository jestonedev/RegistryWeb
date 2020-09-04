using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
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
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            var vm = reportService.GetViewModel();
            return View("Index", vm);
        }

        public IActionResult GetTenancyStatistic(TenancyStatisticModalFilter modalFilter)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            try
            {
                byte[] file;
                if (modalFilter.IdReportType == 0)
                {
                    file = reportService.GetTenancyStatistic(modalFilter);
                    return File(file, odsMime, "Статистика");
                }
                file = reportService.GetTenancyStatisticForCoMSReporter(modalFilter);
                return File(file, odtMime, "Статистика для МКУ ЦПМУ");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetTenancyOrder(TenancyOrderModalFilter modalFilter)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            try
            {
                var file = reportService.GetTenancyOrder(modalFilter);
                return File(file, odtMime, "Распоряжения на найм жилья");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetTenancyNotifiesList(DateTime? dateFrom, DateTime? dateTo)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            try
            {
                var file = reportService.GetTenancyNotifiesList(dateFrom, dateTo);
                return File(file, odsMime, "Уведомление по счетчикам (пер)");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}