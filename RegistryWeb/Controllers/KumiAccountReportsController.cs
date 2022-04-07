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
using RegistryDb.Models.Entities.Payments;

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
                var personInfo = dataService.GetInfoForReport(idAccount);

                if (payment == null)
                  {
                      return Error(string.Format("Отсутствуют начисления по лицевому счету № {0}", idAccount));
                  }

                   var file = reportService.CalDept(payment, personInfo, dateFrom, dateTo, fileFormat);
                  return File(file, fileFormat == 1 ? xlsxMime : odsMime,
                      string.Format(@"Расчет суммы задолженности (лицевой счет № {0}).{1}", idAccount, fileFormat == 1 ? "xlsx" : "ods"));
              }
              catch (Exception ex)
              {
                  return Error(ex.Message);
              }
        }

        public IActionResult InvoiceGenerator(int? idAccount, DateTime onDate, string invoiceAction, string textmessage)
        {
            var results = new Dictionary<int, IEnumerable<string>>();
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return Json(new { ErrorCode = -8, results });
            List<int> ids = new List<int>();
            if (idAccount == null)
            {
                ids = GetSessionIds();
            }
            else
            {
                ids.Add(idAccount.Value);
            }

            if (!ids.Any())
                return Json(new { ErrorCode = -8, results });

            var invoices = new List<InvoiceGeneratorParam>();

            foreach (int id in ids)
                invoices.Add(dataService.GetInvoiceGeneratorParam(id, onDate, textmessage));

            
            var emptyPaymentsInvoices = invoices.FindAll(x => x.Tenant == null && x.Address == "");
            if (emptyPaymentsInvoices.Any())
            {
                results.Add(-6, emptyPaymentsInvoices.Select(s => s.Account).AsEnumerable());

                if (invoiceAction == "Send")
                    foreach (var emptyPayment in emptyPaymentsInvoices)
                    {
                        dataService.AddLogInvoiceGenerator(dataService.InvoiceGeneratorParamToLog(emptyPayment, -6));
                    }
            }

            if (invoiceAction == "Send")
            {
                var emptyEmailsInvoices = invoices.FindAll(x => (x.Tenant != null || x.Address != "") && !x.Emails.Any());
                if (emptyEmailsInvoices.Any())
                {
                    results.Add(-7, emptyEmailsInvoices.Select(s => s.Account).AsEnumerable());
                    foreach (var emptyEmail in emptyEmailsInvoices)
                    {
                        dataService.AddLogInvoiceGenerator(dataService.InvoiceGeneratorParamToLog(emptyEmail, -7));
                    }
                }
            }

            if (invoiceAction == "Export" && results.Any())
            {
                return GenerateInvoiceError(results);
            }/**/

            var correctInvoices = invoices.FindAll(x => (x.Tenant != null || x.Address != "") &&
                ((invoiceAction == "Send" && x.Emails.Any()) || invoiceAction == "Export"));
            if (correctInvoices.Count() > 0)
            {
                try
                {
                    var destDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", Guid.NewGuid().ToString());
                    var resultInvoces = reportService.GenerateInvoices(correctInvoices, invoiceAction, destDirectory);
                    var resultsGroupedByErrorCode = resultInvoces.GroupBy(t => t.Value)
                         .ToDictionary(t => t.Key, t => t.Select(r => r.Key).ToList());

                    foreach (var resultGroup in resultsGroupedByErrorCode)
                    {
                        var accounts = new List<string>();
                        foreach (var invoiceArguments in resultsGroupedByErrorCode.Values)
                        {
                            for (var i = 0; i < invoiceArguments.Count; i++)
                            {
                                accounts.Add(invoiceArguments[i]["--account"].ToString());

                                if (invoiceAction == "Send")
                                {
                                    var log = new LogInvoiceGenerator
                                    {
                                        IdAccount = Convert.ToInt32(invoiceArguments[i]["id_account"]),
                                        CreateDate = DateTime.Now,
                                        OnDate = Convert.ToDateTime(invoiceArguments[i]["--on-date"]),
                                        Emails = invoiceArguments[i]["--email"].ToString(),
                                        ResultCode = resultGroup.Key
                                    };
                                    dataService.AddLogInvoiceGenerator(log);
                                }
                            }
                        }
                        results.Add(resultGroup.Key, accounts);
                    }

                    if (invoiceAction == "Export" && results.Where(result => result.Key < 0).Any())
                        return GenerateInvoiceError(results);

                    if (invoiceAction == "Export")
                        return InvoiceExport(destDirectory);
                }
                catch (Exception e)
                {
                    return Json(new { ErrorCode = -9 });
                }
            }
            return Json(new { results });
        }

        private IActionResult InvoiceExport(string destDirectory)
        {
            var files = Directory.GetFiles(destDirectory);
            if (!files.Any())
            {
                return Error("Неизвестная ошибка. Не сформировались файлы для скачивания");
            }
            if (files.Length == 1)
            {
                var fileName = files[0];
                var fileInfo = new FileInfo(fileName);
                var file = System.IO.File.ReadAllBytes(fileName);
                Directory.Delete(destDirectory, true);
                return File(file, pdfMime, fileInfo.Name);
            }
            else
            {
                var destZipFile = destDirectory + ".zip";
                var fileInfo = new FileInfo(destZipFile);
                ZipFile.CreateFromDirectory(destDirectory, destZipFile);
                Directory.Delete(destDirectory, true);
                var file = System.IO.File.ReadAllBytes(destZipFile);
                System.IO.File.Delete(destZipFile);
                return File(file, zipMime, fileInfo.Name);
            }
        }

        private IActionResult GenerateInvoiceError(Dictionary<int, IEnumerable<string>> results)
        {
            var error = "";
            foreach (var result in results.Where(r => r.Key != 0))
            {
                switch (result.Key)
                {
                    case -1:
                        error += "Ошибка при сохранении qr - кода. ЛС № ";
                        break;
                    case -2:
                        error += "Ошибка сохранения html - файла. ЛС № ";
                        break;
                    case -3:
                        error += "Ошибка конвертации html в pdf. ЛС № ";
                        break;
                    case -4:
                        error += "Ошибка отправки сообщения. ЛС № ";
                        break;
                    case -5:
                        error += "Ошибка удаления временных файлов. ЛС № ";
                        break;
                    case -6:
                        error += "Отсутствует платеж на указанную дату. ЛС № ";
                        break;
                    case -7:
                        error += "Отсутствует электронная почта для отправки. ЛС № ";
                        break;
                    case -8:
                        error += "Недостаточно прав на данную операцию. ЛС № ";
                        break;
                    default:
                        error += "Неизвестная ошибка. ЛС № ";
                        break;
                }
                if (result.Value.Count() > 8)
                    error += result.Value.Take(8).Aggregate((acc, v) => acc + ", " + v) + "... ";
                else
                    error += result.Value.Aggregate((acc, v) => acc + ", " + v) + ". ";
            }
            return Error(error);
        }
    }
}
