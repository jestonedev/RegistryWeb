using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
    }
}
