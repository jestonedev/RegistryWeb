using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Payments;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RegistryWeb.ReportServices
{
    public class KumiPaymentsReportService : ReportService
    {
        private readonly SecurityService securityService;

        public KumiPaymentsReportService(IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(config, httpContextAccessor)
        {
            this.securityService = securityService;
        }

        public byte[] GetPaymentOrder(int idPayment)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_payment", idPayment }
            };
            var fileName = "registry\\kumi_accounts\\payment_order";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }
    }
}
