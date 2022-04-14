﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class KumiPaymentsReportsController : SessionController<TenancyProcessesFilter>
    {
        private readonly KumiPaymentsReportService reportService;
        private readonly SecurityService securityService;
        private const string odsMime = "application/vnd.oasis.opendocument.spreadsheet";
        private const string xlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public KumiPaymentsReportsController(KumiPaymentsReportService reportService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.securityService = securityService;
        }

        public IActionResult GetPaymentOrder(int idPayment)
        {
            if (!securityService.HasPrivilege(Privileges.AccountsRead))
                return View("NotAccess");
            try
            {
                var file = reportService.GetPaymentOrder(idPayment);
                return File(file, xlsxMime, string.Format(@"Платежное поручение № {0}.xlsx", idPayment));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}