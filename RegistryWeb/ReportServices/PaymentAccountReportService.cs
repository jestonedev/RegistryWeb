using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        internal byte[] CalDept(Payment lastPayment, DateTime dateFrom, DateTime dateTo, int fileFormat)
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

        internal byte[] ExportPayments(List<int> idAccounts)
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

        internal Dictionary<Dictionary<string, object>, int> GenerateInvoices(List<InvoiceGeneratorParam> invoices, string action, string destDirectory = null)
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
                };
                if (action == "Send")
                    arguments.Add("--email", invoice.Emails.Aggregate((x, y) => x + "," + y));
                else
                    arguments.Add("--move-to-filename", Path.Combine(destDirectory, string.Format("Счет-извещение по ЛС № {0}.pdf", invoice.Account)));
                parametrs.Add(arguments);
            }
            return GenerateInvoices(parametrs);
        }
    }
}
