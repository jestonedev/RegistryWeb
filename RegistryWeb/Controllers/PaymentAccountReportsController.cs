using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RegistryWeb.DataServices;
using RegistryWeb.Models;
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
                return File(file, odtMime, string.Format(@"Запрос в БКС{0}.odt", idAccount == 0 ? "" : string.Format(" (лицевой счет № {0})", idAccount)));
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
               return File(file, fileFormat == 1 ? xlsxMime : odsMime, 
                   string.Format(@"Расчет суммы задолженности (лицевой счет № {0}).{1}", idAccount, fileFormat == 1 ? "xlsx" : "ods"));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public JsonResult InvoiceGenerator(int idAccount, DateTime onDate)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return Json(new { ErrorCode = -8 });
            try
            {
                var paymentOnDate = dataService.GetPaymentOnDate(idAccount, onDate);
                var inv = new LogInvoiceGenerator
                {
                    CreateDate = DateTime.Now,
                    OnDate = onDate
                };

                if (paymentOnDate.Tenant == null)
                {
                    inv.IdAccount = idAccount;
                    inv.Emails="";
                    inv.Result_code = -6;
                    
                    return Json(new { ErrorCode = -6 });
                }
                if (!paymentOnDate.Emails.Any())
                {
                    inv.IdAccount = idAccount;
                    inv.Emails = "";
                    inv.Result_code = -7;
                    
                    return Json(new { ErrorCode = -7 });
                }

                var code = reportService.InvoiceGenerator(paymentOnDate);
            
                inv.IdAccount = paymentOnDate.IdAcconut;
                inv.Emails = string.Join(", ", paymentOnDate.Emails).ToString();
                inv.Result_code = code;
                dataService.AddLIG(inv);

                return Json(new { ErrorCode = code });
            }
            catch (Exception ex)
            {
                return Json(new { ErrorCode = -9 });
            }
        }

        public JsonResult InvoicesGenerator(DateTime onDate)
        {
            List<int> ids = GetSessionIds();
            var jsonResults = new Dictionary<int, IEnumerable<string>>();

            if (!ids.Any())
                return Json(new { errorCode = -8, jsonResults });

            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return Json(new { errorCode = -8, jsonResults });

            var invoices = new List<InvoiceGeneratorParam>();
            try
            {
                foreach(int idAccount in ids)                
                    invoices.Add(dataService.GetPaymentOnDate(idAccount, onDate));

                var nuls = invoices.FindAll(x => x.Tenant==null);
                var emailfree = invoices.FindAll(x=> x.Tenant!=null && !x.Emails.Any());
                if (nuls.Any())
                {
                    jsonResults.Add(-6, nuls.Select(s=>s.Account).AsEnumerable());

                    foreach(var n in nuls)
                    {
                        //код для добавл-я записи в бд
                        var inv = new LogInvoiceGenerator
                        {
                            IdAccount = n.IdAcconut,
                            CreateDate = DateTime.Now,
                            OnDate = n.OnData,
                            Emails = string.Join(", ", n.Emails).ToString(),
                            Result_code = -6
                        };
                        dataService.AddLIG(inv);
                    }
                }
                if (emailfree.Any())
                {
                    jsonResults.Add(-7, emailfree.Select(s=>s.Account).AsEnumerable());

                    foreach (var ef in emailfree)
                    {
                        //код для добавл-я записи в бд
                        var inv = new LogInvoiceGenerator
                        {
                            IdAccount = ef.IdAcconut,
                            CreateDate = DateTime.Now,
                            OnDate = ef.OnData,
                            Emails = string.Join(", ", ef.Emails).ToString(),
                            Result_code = -7
                        };
                        dataService.AddLIG(inv);
                    }
                }

                var listi=invoices.FindAll(x=>x!=null && x.Emails.Any());
                if (listi.Count() > 0 && listi.Count() <= invoices.Count())
                {
                    var result = reportService.InvoicesGenerator(listi);

                    var resultgr = result.GroupBy(t => t.Value)
                             .ToDictionary(t=>t.Key, t=>t.Select(r=>r.Key).ToList());

                    foreach(var u in resultgr)
                    {
                        int i;  var h = "";
                        foreach(var res in resultgr.Values)
                        {
                            for (i = 0; i < res.Count; i++)
                            {
                                h += res[i]["--account"];
                                if (i!=res.Count-1)
                                    h += ", ";

                                //код для добавл-я записи в бд
                                var inv = new LogInvoiceGenerator
                                {
                                    IdAccount = Convert.ToInt32(res[i]["id_account"]),
                                    CreateDate = DateTime.Now,
                                    OnDate = Convert.ToDateTime(res[i]["--on-date"]),
                                    Emails = res[i]["--email"].ToString(),
                                    Result_code=u.Key
                                };
                                dataService.AddLIG(inv);
                            }
                        }
                        jsonResults.Add(u.Key, h.Split(',').AsEnumerable());
                    }
                }
                return Json(jsonResults);
            }
            catch (Exception ex)
            {
                jsonResults.Clear();
                jsonResults.Add(-9, null);
                return Json(jsonResults);
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
                return File(file, odsMime, "Экспорт данных.ods");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}