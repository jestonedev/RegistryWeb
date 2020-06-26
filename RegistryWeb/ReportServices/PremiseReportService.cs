using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RegistryWeb.SecurityServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ReportServices
{
    public class PremiseReportService : ReportService
    {
        private readonly SecurityService securityService;

        public PremiseReportService(IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(config, httpContextAccessor)
        {
            this.securityService = securityService;
        }

        public byte[] ExcerptPremise(int idPremise, string excerptNumber, DateTime excerptDateFrom, int signer)
        {
            var arguments = new Dictionary<string, object>
            {
                { "ids", idPremise },
                { "excerpt_type", 1 },
                { "excerpt_number", excerptNumber },
                { "excerpt_date_from", excerptDateFrom.ToString("dd.MM.yyyy") },
                { "signer", signer }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\registry\\excerpt");
            return DownloadFile(fileNameReport);
        }

        public byte[] ExcerptSubPremise(int idPremise, string excerptNumber, DateTime excerptDateFrom, int signer)
        {
            var arguments = new Dictionary<string, object>
            {
                { "ids", idPremise },
                { "excerpt_type", 2 },
                { "excerpt_number", excerptNumber },
                { "excerpt_date_from", excerptDateFrom.ToString("dd.MM.yyyy") },
                { "signer", signer }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\registry\\excerpt");
            return DownloadFile(fileNameReport);
        }

        public byte[] ExcerptMunSubPremises(int idPremise, string excerptNumber, DateTime excerptDateFrom, int signer)
        {
            var arguments = new Dictionary<string, object>
            {
                { "ids", idPremise },
                { "excerpt_type", 3 },
                { "excerpt_number", excerptNumber },
                { "excerpt_date_from", excerptDateFrom.ToString("dd.MM.yyyy") },
                { "signer", signer }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\registry\\excerpt");
            return DownloadFile(fileNameReport);
        }

        public byte[] PremiseNoticeToBks(int idPremise, string actionText, int paymentType, int signer)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id", idPremise },
                { "notice_type", 1 },
                { "executor", securityService.User.UserName },
                { "text", actionText },
                { "payment_type", paymentType },
                { "signer", signer }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\registry\\notice_BKS");
            return DownloadFile(fileNameReport);
        }

        public byte[] SubPremiseNoticeToBks(int idSubPremise, string actionText, int paymentType, int signer)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id", idSubPremise },
                { "notice_type", 2 },
                { "executor", securityService.User.UserName },
                { "text", actionText },
                { "payment_type", paymentType },
                { "signer", signer }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\registry\\notice_BKS");
            return DownloadFile(fileNameReport);
        }

        public byte[] PremiseArea(int idPremise)
        {
            var arguments = new Dictionary<string, object>
            {
                { "filter", idPremise }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\registry\\area_premises");
            return DownloadFile(fileNameReport);
        }
    }
}
