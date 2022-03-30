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

        public byte[] CalDept(KumiCharge lastPayment, TenancyPerson person,  string totalArea, int prescribed, DateTime dateFrom, DateTime dateTo, int fileFormat)
        {
            var arguments = new Dictionary<string, object>
            {
                { "date_from", dateFrom.ToString("yyyy.MM.dd")},
                { "date_to", dateTo.ToString("yyyy.MM.dd")},
                { "id_account", lastPayment.Account.IdAccount },
                { "account", lastPayment.Account.Account },
                { "tenant", string.Concat(person.Surname," ", person.Name, " ", person.Patronymic) },
                { "raw_address", lastPayment.Account.KumiAccountAddressNavigation.Address },
                { "prescribed", prescribed },
                { "total_area", totalArea },
                { "templateFileName", activityManagerPath + "templates\\registry\\kumi_report_test\\amount_debt_KUMI." + (fileFormat == 1 ? "xlsx" : "ods") },
            };
            var fileName = "registry\\kumi_report_test\\amount_debt_KUMI";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }
    }
}
