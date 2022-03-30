using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryDb.Models;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class KumiAccountReportsController : SessionController<PaymentsFilter>
    {
        private readonly KumiAccountReportService reportService;
        private readonly KumiAccountReportsDataService dataService;
        private readonly SecurityService securityService;
        private readonly TenancyProcessesDataService processesDataService;
        private const string zipMime = "application/zip";
        private const string pdfMime = "application/pdf";
        private const string odtMime = "application/vnd.oasis.opendocument.text";
        private const string odsMime = "application/vnd.oasis.opendocument.spreadsheet";
        private const string xlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private const string docxMime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public KumiAccountReportsController(KumiAccountReportService reportService, KumiAccountReportsDataService dataService, SecurityService securityService, TenancyProcessesDataService processesDataService)
        {
            this.reportService = reportService;
            this.dataService = dataService;
            this.securityService = securityService;
            this.processesDataService = processesDataService;

            nameFilteredIdsDict = "filteredAccountsIdsDict";
            nameIds = "idAccounts";
            nameMultimaster = "AccountsReports";
        }
        public IActionResult GetCalDept(int idAccount, DateTime dateFrom, DateTime dateTo, int fileFormat)
        {
              if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
              try
              { 
                  var payment = dataService.GetLastPayment(idAccount);
                  var person = dataService.GetPersonPayment(idAccount);
                  var totalArea =string.Join(";", processesDataService.GetRentObjects(payment.Account.TenancyProcesses
                  .Where(c => c.IdProcess == person.IdProcess))
                  .Select(t => t.Value.Select(c =>  c.TotalArea).ToList()).ToList()
                  .SelectMany(c => c).ToList());
                var prescribed = processesDataService.GetTenancyProcess(person.IdProcess).TenancyPersons.Count; 
                if (payment == null)
                  {
                      return Error(string.Format("Отсутствуют начисления по лицевому счету № {0}", idAccount));
                  }

                   var file = reportService.CalDept(payment, person, totalArea, prescribed, dateFrom, dateTo, fileFormat);
                  return File(file, fileFormat == 1 ? xlsxMime : odsMime,
                      string.Format(@"Расчет суммы задолженности (лицевой счет № {0}).{1}", idAccount, fileFormat == 1 ? "xlsx" : "ods"));
              }
              catch (Exception ex)
              {
                  return Error(ex.Message);
              }
        }
    }
}
