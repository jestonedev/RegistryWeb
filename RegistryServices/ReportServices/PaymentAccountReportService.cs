using InvoiceGenerator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RegistryDb.Models.Entities.Payments;
using RegistryWeb.SecurityServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RegistryWeb.ReportServices
{
    public class PaymentAccountReportService : ReportService
    {
        private readonly SecurityService securityService;

        public PaymentAccountReportService(IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(config, httpContextAccessor)
        {
            this.securityService = securityService;
        }

        public byte[] RequestToBks(List<int> idAccounts, int idSigner, DateTime dateValue)
        {
            var tmpFileName = Path.GetTempFileName();
            var idAccountsStr = idAccounts.Select(id => id.ToString()).Aggregate((x, y) => x + "," + y);
            using (var sw = new StreamWriter(tmpFileName))
                sw.Write(idAccountsStr);
            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", tmpFileName },
                { "request_date_from", dateValue.ToString("dd.MM.yyyy") },
                { "signer", idSigner },
                { "executor", securityService.Executor?.ExecutorLogin?.Split("\\")[1] }
            };
            var fileName = "registry\\claims\\request_BKS";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] CalDept(Payment lastPayment, DateTime dateFrom, DateTime dateTo, int fileFormat)
        {
            var arguments = new Dictionary<string, object>
            {
                { "date_from", dateFrom.ToString("yyyy.MM.dd")},
                { "date_to", dateTo.ToString("yyyy.MM.dd")},
                { "id_account", lastPayment.PaymentAccountNavigation.IdAccount },
                { "account", lastPayment.PaymentAccountNavigation.Account },
                { "tenant", lastPayment.Tenant },
                { "raw_address", lastPayment.PaymentAccountNavigation.RawAddress },
                { "prescribed", lastPayment.Prescribed },
                { "total_area", lastPayment.TotalArea },
                { "templateFileName", activityManagerPath + "templates\\registry\\registry\\amount_debt_KUMI." + (fileFormat == 1 ? "xlsx" : "ods") },
            };
            var fileName = "registry\\registry\\amount_debt_KUMI";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ExportPayments(List<int> idAccounts)
        {
            string columnHeaders;
            string columnPatterns;

            columnHeaders = "[{\"columnHeader\":\"Состояние на дату\"},{\"columnHeader\":\"Дата последнего начисления\"},"+
                "{\"columnHeader\":\"СРН\"},{\"columnHeader\":\"Адрес по БКС\"},{\"columnHeader\":\"Адрес в реестре ЖФ\"},"+
                "{\"columnHeader\":\"Лицевой счет\"},{\"columnHeader\":\"Наниматель\"},"+
                "{\"columnHeader\":\"Текущее состояние посл. иск. работы\"},{\"columnHeader\":\"Период посл. иск. работы с\"},{\"columnHeader\":\"Период посл. иск. работы по\"}," +
                "{\"columnHeader\":\"Общая площадь\"},{\"columnHeader\":\"Жилая площадь\"},{\"columnHeader\":\"Прописано\"}," +
                "{\"columnHeader\":\"Сальдо вх.\"},{\"columnHeader\":\"Сальдо вх. найм\"},{\"columnHeader\":\"Пени (вх.)\"},"+
                "{\"columnHeader\":\"Сальдо вх. ДГИ\"},{\"columnHeader\":\"Сальдо вх. Падун\"},{\"columnHeader\":\"Сальдо вх. ПКК\"},"+
                "{\"columnHeader\":\"Начисление итого\"},{\"columnHeader\":\"Начисление найм\"},{\"columnHeader\":\"Начисление пени\"},"+
                "{\"columnHeader\":\"Начисление ДГИ\"},{\"columnHeader\":\"Начисление Падун\"},{\"columnHeader\":\"Начисление ПКК\"},"+
                "{\"columnHeader\":\"Перенос сальдо\"},{\"columnHeader\":\"Перерасчет найм\"},{\"columnHeader\":\"Перерасчет пени\"},"+
                "{\"columnHeader\":\"Перерасчет ДГИ\"},{\"columnHeader\":\"Перерасчет Падун\"},{\"columnHeader\":\"Перерасчет ПКК\"},"+
                "{\"columnHeader\":\"Оплата найм\"},{\"columnHeader\":\"Оплата пени\"},{\"columnHeader\":\"Оплата ДГИ\"},"+
                "{\"columnHeader\":\"Оплата Падун\"},{\"columnHeader\":\"Оплата ПКК\"},{\"columnHeader\":\"Сальдо исх.\"},"+
                "{\"columnHeader\":\"Сальдо исх. найм\"},{\"columnHeader\":\"Пени исх.\"},{\"columnHeader\":\"Сальдо исх. ДГИ\"},"+
                "{\"columnHeader\":\"Сальдо исх. Падун\"},{\"columnHeader\":\"Сальдо исх. ПКК\"}]";
            columnPatterns = "[{\"columnPattern\":\"$column0$\"},{\"columnPattern\":\"$column1$\"},{\"columnPattern\":\"$column2$\"},"+
                "{\"columnPattern\":\"$column3$\"},{\"columnPattern\":\"$column4$\"},{\"columnPattern\":\"$column5$\"},{\"columnPattern\":\"$column6$\"},"+
                "{\"columnPattern\":\"$column7$\"},{\"columnPattern\":\"$column8$\"},{\"columnPattern\":\"$column9$\"},{\"columnPattern\":\"$column10$\"},"+
                "{\"columnPattern\":\"$column11$\"},{\"columnPattern\":\"$column12$\"},{\"columnPattern\":\"$column13$\"},{\"columnPattern\":\"$column14$\"},"+
                "{\"columnPattern\":\"$column15$\"},{\"columnPattern\":\"$column16$\"},{\"columnPattern\":\"$column17$\"},{\"columnPattern\":\"$column18$\"},"+
                "{\"columnPattern\":\"$column19$\"},{\"columnPattern\":\"$column20$\"},{\"columnPattern\":\"$column21$\"},{\"columnPattern\":\"$column22$\"},"+
                "{\"columnPattern\":\"$column23$\"},{\"columnPattern\":\"$column24$\"},{\"columnPattern\":\"$column25$\"},{\"columnPattern\":\"$column26$\"},"+
                "{\"columnPattern\":\"$column27$\"},{\"columnPattern\":\"$column28$\"},{\"columnPattern\":\"$column29$\"},{\"columnPattern\":\"$column30$\"},"+
                "{\"columnPattern\":\"$column31$\"},{\"columnPattern\":\"$column32$\"},{\"columnPattern\":\"$column33$\"},{\"columnPattern\":\"$column34$\"},"+
                "{\"columnPattern\":\"$column35$\"},{\"columnPattern\":\"$column36$\"},{\"columnPattern\":\"$column37$\"},{\"columnPattern\":\"$column38$\"},"+
                "{\"columnPattern\":\"$column39$\"},{\"columnPattern\":\"$column40$\"},{\"columnPattern\":\"$column41$\"}]";

            var fileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(fileName))
                sw.Write(string.Format("(id_account IN ({0}))", AccountIdsToString(idAccounts)));

            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", fileName },
                { "type", "5"},
                { "executor", securityService.User.UserName.Replace("PWR\\", "") },
                { "columnHeaders", columnHeaders },
                { "columnPatterns", columnPatterns },
                { "orderColumn", "id_account" }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\export");
            return DownloadFile(fileNameReport);
        }

        private string AccountIdsToString(List<int> idAccounts)
        {
            return idAccounts.Select(r => r.ToString()).Aggregate((v, acc) => v + "," + acc);
        }

        public Dictionary<Dictionary<string, object>, int> GenerateInvoices(List<InvoiceGeneratorParam> invoices, string action, string destDirectory = null)
        {
            var parametrs = new List<Dictionary<string, object>>();
            if (action == "Export")
            {
                if (Directory.Exists(destDirectory)) Directory.Delete(destDirectory);
                Directory.CreateDirectory(destDirectory);
            }
            for (var i =0; i < invoices.Count; i++)
            {
                var invoice = invoices[i];
                var arguments = new Dictionary<string, object>
                {
                    { "--address", invoice.Address },
                    { "--id-account", invoice.IdAccount },
                    { "--account", invoice.Account },
                    { "--tenant", invoice.Tenant },
                    { "--on-date", invoice.OnDate.ToString("dd.MM.yyyy")},
                    { "--balance-input", invoice.BalanceInput },
                    { "--charging-tenancy", invoice.ChargingTenancy },
                    { "--charging-penalty", invoice.ChargingPenalty },
                    { "--payed", invoice.Payed },
                    { "--recalc-tenancy", invoice.RecalcTenancy },
                    { "--recalc-penalty", invoice.RecalcPenalty },
                    { "--balance-output", invoice.BalanceOutput },
                    { "--total-area", invoice.TotalArea },
                    { "--prescribed", invoice.Prescribed },
                    { "--message", invoice.TextMessage },
                };
                if (action == "Send")
                    arguments.Add("--email", invoice.Emails.Aggregate((x, y) => x + "," + y));
                else
                {
                    if (invoices.Count - 1 <= i)
                    {
                        arguments.Add("--move-to-filename", Path.Combine(destDirectory, string.Format("Счет-извещение по ЛС № {0}.pdf", invoice.Account)));
                    }
                    else
                    {
                        var preInvoce = invoice;
                        i++;
                        invoice = invoices[i];
                        arguments.Add("--address-2", invoice.Address);
                        arguments.Add("--account-2", invoice.Account);
                        arguments.Add("--tenant-2", invoice.Tenant);
                        arguments.Add("--on-date-2", invoice.OnDate.ToString("dd.MM.yyyy"));
                        arguments.Add("--balance-input-2", invoice.BalanceInput);
                        arguments.Add("--charging-tenancy-2", invoice.ChargingTenancy);
                        arguments.Add("--charging-penalty-2", invoice.ChargingPenalty);
                        arguments.Add("--payed-2", invoice.Payed);
                        arguments.Add("--recalc-tenancy-2", invoice.RecalcTenancy);
                        arguments.Add("--recalc-penalty-2", invoice.RecalcPenalty);
                        arguments.Add("--balance-output-2", invoice.BalanceOutput);
                        arguments.Add("--total-area-2", invoice.TotalArea);
                        arguments.Add("--prescribed-2", invoice.Prescribed);
                        arguments.Add("--move-to-filename", Path.Combine(destDirectory, string.Format("Счет-извещение по ЛС № {0}, {1}.pdf", preInvoce.Account, invoice.Account)));
                    }
                }
                parametrs.Add(arguments);
            }
            return GenerateInvoices(parametrs);
        }
    }
}
