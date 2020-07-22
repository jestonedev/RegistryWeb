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


//___________для массовых______________
        public byte[] ExcerptPremises(string idPremises, string excerptNumber, DateTime excerptDateFrom, int signer)
        {
            var arguments = new Dictionary<string, object>
            {
                { "ids", idPremises },
                { "excerpt_type", 4 },
                { "excerpt_date_from", excerptDateFrom.ToString("dd.MM.yyyy") },
                { "excerpt_number", excerptNumber },
                { "executor", securityService.User.UserName },
                { "signer", signer }
            };
            //var fileNameReport = GenerateReport(arguments, "registry\\registry\\excerpt");
            var fileNameReport = GenerateReport(arguments, @"D:\Projects\Всячина проектов\RegistryWeb\Отчёты\premises_mx");
            return DownloadFile(fileNameReport);
        }

        public byte[] PremisesArea(string idPremises)
        {
            var arguments = new Dictionary<string, object>
            {
                { "filter", idPremises }
            };
            //var fileNameReport = GenerateReport(arguments, "registry\\registry\\area_premises");
            var fileNameReport = GenerateReport(arguments, @"D:\Projects\Всячина проектов\RegistryWeb\Отчёты\area_premises");
            return DownloadFile(fileNameReport);
        }

        public byte[] MassActPremises(string idPremises, DateTime actDate, bool isNotResides, string commision, int clerk)
        {
            var arguments = new Dictionary<string, object>
            {
                { "filter", idPremises },
                { "acttype", 1 },
                { "executor", securityService.User.UserName },
                { "date_act", actDate },
                { "is_not_resides", isNotResides },
                { "ids_commission", commision },
                { "id_clerk", clerk }
            };
            //var fileNameReport = GenerateReport(arguments, "registry\\registry\\act_residence");
            var fileNameReport = GenerateReport(arguments, @"D:\Projects\Всячина проектов\RegistryWeb\Отчёты\act_residence");
            return DownloadFile(fileNameReport);
        }




    }
}
