using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.Tenancies;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace RegistryWeb.ReportServices
{
    public class KumiAccountReportService : ReportService
    {
        private readonly SecurityService securityService;

        public KumiAccountReportService(IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(config, httpContextAccessor)
        {
            this.securityService = securityService;
        }

        public byte[] CalDept(KumiCharge lastPayment, Dictionary<string, object> personInfo, DateTime dateFrom, DateTime dateTo, int fileFormat)
        {
            var arguments = new Dictionary<string, object>
            {
                { "date_from", dateFrom.ToString("yyyy.MM.dd")},
                { "date_to", dateTo.ToString("yyyy.MM.dd")},
                { "id_account", lastPayment.Account.IdAccount },
                { "account", lastPayment.Account.Account },
                { "tenant", personInfo.Where(c => c.Key.Contains("tenant")).Select(c => c.Value).FirstOrDefault().ToString()},
                { "raw_address",personInfo.Where(c => c.Key.Contains("totalArea")).Select(c => c.Value).FirstOrDefault().ToString().Replace(',', '.')},
                { "prescribed", (int)personInfo.Where(c => c.Key.Contains("prescribed")).Select(c => c.Value).FirstOrDefault() },
                { "total_area", personInfo.Where(c => c.Key.Contains("totalArea")).Select(c => c.Value).FirstOrDefault().ToString().Replace(',', '.') },
                { "templateFileName", activityManagerPath + "templates\\registry\\kumi_accounts\\amount_debt_KUMI." + (fileFormat == 1 ? "xlsx" : "ods") },
            };
            var fileName = "registry\\kumi_accounts\\amount_debt_KUMI";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public Dictionary<Dictionary<string, object>, int> GenerateInvoices(List<InvoiceGeneratorParam> invoices, string action, string destDirectory = null)
        {
            var parametrs = new List<Dictionary<string, object>>();
            if (action == "Export")
            {
                if (Directory.Exists(destDirectory)) Directory.Delete(destDirectory);
                Directory.CreateDirectory(destDirectory);
            }
            foreach (var invoice in invoices)
            {
                var arguments = new Dictionary<string, object>
                {
                    { "id_account", invoice.IdAcconut },
                    { "--address", invoice.Address },
                    { "--account", invoice.Account },
                    { "--tenant", invoice.Tenant },
                    { "--on-date", invoice.OnData.ToString("dd.MM.yyyy")},
                    { "--balance-input", invoice.BalanceInput },
                    { "--charging", invoice.Charging },
                    { "--payed", invoice.Payed },
                    { "--recalc", invoice.Recalc },
                    { "--balance-output", invoice.BalanceOutput },
                    { "--total-area", invoice.TotalArea },
                    { "--prescribed", invoice.Prescribed },
                    { "--message", invoice.TextMessage },
                };
                if (action == "Send")
                    arguments.Add("--email", invoice.Emails.Aggregate((x, y) => x + "," + y));
                else
                    arguments.Add("--move-to-filename", Path.Combine(destDirectory, string.Format("Счет-извещение по ЛС № {0}.pdf", invoice.Account)));
                parametrs.Add(arguments);
            }
            return GenerateInvoices(parametrs);
        }

        public byte[] ExportAccounts(List<int> idAccounts)
        {
            string columnHeaders;
            string columnPatterns;

            columnHeaders = "[{\"columnHeader\":\"Состояние на дату\"},{\"columnHeader\":\"Дата последнего начисления\"}," +
                "{\"columnHeader\":\"Адрес\"}," +
                "{\"columnHeader\":\"Лицевой счет\"},{\"columnHeader\":\"Наниматель\"}," +
                "{\"columnHeader\":\"Текущее состояние посл. иск. работы\"},{\"columnHeader\":\"Период посл. иск. работы с\"},{\"columnHeader\":\"Период посл. иск. работы по\"}," +
                "{\"columnHeader\":\"Общая площадь\"},{\"columnHeader\":\"Прописано\"}," +
                "{\"columnHeader\":\"Сальдо вх.\"},{\"columnHeader\":\"Сальдо вх. найм\"},{\"columnHeader\":\"Пени (вх.)\"}," +
                "{\"columnHeader\":\"Начисление итого\"},{\"columnHeader\":\"Начисление найм\"},{\"columnHeader\":\"Начисление пени\"}," +
                "{\"columnHeader\":\"Перерасчет найм\"},{\"columnHeader\":\"Перерасчет пени\"}," +
                "{\"columnHeader\":\"Оплата найм\"},{\"columnHeader\":\"Оплата пени\"},{\"columnHeader\":\"Сальдо исх.\"}," +
                "{\"columnHeader\":\"Сальдо исх. найм\"},{\"columnHeader\":\"Пени исх.\"}]";
            columnPatterns = "[{\"columnPattern\":\"$column0$\"},{\"columnPattern\":\"$column1$\"},{\"columnPattern\":\"$column2$\"}," +
                "{\"columnPattern\":\"$column3$\"},{\"columnPattern\":\"$column4$\"},{\"columnPattern\":\"$column5$\"},{\"columnPattern\":\"$column6$\"}," +
                "{\"columnPattern\":\"$column7$\"},{\"columnPattern\":\"$column8$\"},{\"columnPattern\":\"$column9$\"},{\"columnPattern\":\"$column10$\"}," +
                "{\"columnPattern\":\"$column11$\"},{\"columnPattern\":\"$column12$\"},{\"columnPattern\":\"$column13$\"},{\"columnPattern\":\"$column14$\"}," +
                "{\"columnPattern\":\"$column15$\"},{\"columnPattern\":\"$column16$\"},{\"columnPattern\":\"$column17$\"},{\"columnPattern\":\"$column18$\"}," +
                "{\"columnPattern\":\"$column19$\"},{\"columnPattern\":\"$column20$\"},{\"columnPattern\":\"$column21$\"},{\"columnPattern\":\"$column22$\"}]";

            var fileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(fileName))
                sw.Write(string.Format("(id_account IN ({0}))", idAccounts.Select(r => r.ToString()).Aggregate((v, acc) => v + "," + acc)));

            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", fileName },
                { "type", "6"},
                { "executor", securityService.User.UserName.Replace("PWR\\", "") },
                { "columnHeaders", columnHeaders },
                { "columnPatterns", columnPatterns },
                { "orderColumn", "id_account" }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\export");
            return DownloadFile(fileNameReport);
        }
    }
}
