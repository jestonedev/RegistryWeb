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
    [HasPrivileges(Privileges.AccountsRead)]
    [DefaultResponseOnException(typeof(Exception))]
    public class KumiPaymentsReportsController : SessionController<TenancyProcessesFilter>
    {
        private readonly KumiPaymentsReportService reportService;
        private readonly SecurityService securityService;

        public KumiPaymentsReportsController(KumiPaymentsReportService reportService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.securityService = securityService;
        }

        public IActionResult GetPaymentOrder(int idPayment)
        {
            var file = reportService.GetPaymentOrder(idPayment);
            return File(file, MimeTypeHelper.XlsxMime, string.Format(@"Платежное поручение № {0}.xlsx", idPayment));
        }
    }
}