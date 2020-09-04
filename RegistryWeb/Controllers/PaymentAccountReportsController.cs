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
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class PaymentAccountReportsController : SessionController<PaymentsFilter>
    {
        private readonly PaymentAccountReportService reportService;
        private readonly PaymentAccountReportsDataService dataService;
        private readonly SecurityService securityService;
        private const string zipMime = "application/zip";
        private const string odtMime = "application/vnd.oasis.opendocument.text";
        private const string odsMime = "application/vnd.oasis.opendocument.spreadsheet";
        private const string xlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private const string docxMime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        public PaymentAccountReportsController(PaymentAccountReportService reportService, PaymentAccountReportsDataService dataService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.dataService = dataService;
            this.securityService = securityService;

            nameFilteredIdsDict = "filteredAccountsIdsDict";
            nameIds = "idAccounts";
            nameMultimaster = "AccountsReports";
        }

        public IActionResult GetRequestToBks(int idAccount, int idSigner, DateTime dateValue)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
            try
            {
                List<int> ids = new List<int>();
                if (idAccount == 0)
                {
                    ids = GetSessionIds();
                }
                else
                {
                    ids.Add(idAccount);
                }

                var file = reportService.RequestToBks(ids, idSigner, dateValue);
                return File(file, odtMime, string.Format(@"Запрос в БКС{0}", idAccount == 0 ? "" : string.Format(" (лицевой счет № {0})", idAccount)));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetCalDept(int idAccount, DateTime dateFrom, DateTime dateTo, int fileFormat)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
            try
            {
                var payment = dataService.GetLastPayment(idAccount);
                if (payment == null)
                {
                    return Error(string.Format("Отсутствуют начисления по лицевому счету № {0}", idAccount));
                }
                var file = reportService.CalDept(payment, dateFrom, dateTo, fileFormat);
               return File(file, fileFormat == 1 ? xlsxMime : odsMime, string.Format(@"Расчет суммы задолженности (лицевой счет № {0})", idAccount));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetPaymentsExport()
        {
            List<int> ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

            try
            {
                var file = reportService.ExportPayments(ids);
                return File(file, odsMime, "Экспорт данных");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}