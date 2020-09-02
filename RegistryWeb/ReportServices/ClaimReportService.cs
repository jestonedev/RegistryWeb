using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RegistryWeb.SecurityServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ReportServices
{
    public class ClaimReportService : ReportService
    {
        private readonly SecurityService securityService;

        public ClaimReportService(IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(config, httpContextAccessor)
        {
            this.securityService = securityService;
        }

        public byte[] TransferToLegal(List<int> idClaims, int idSigner, DateTime dateValue)
        {
            var tmpFileName = Path.GetTempFileName();
            var idClaimsStr = idClaims.Select(id => id.ToString()).Aggregate((x, y) => x + "," + y);
            using (var sw = new StreamWriter(tmpFileName))
                sw.Write(idClaimsStr);
            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", tmpFileName },
                { "request_date_from", dateValue.ToString("dd.MM.yyyy") },
                { "signer", idSigner },
                { "executor", securityService.Executor?.ExecutorLogin?.Split("\\")[1] }
            };
            var fileName = "registry\\claims\\transfer_JD";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
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

        internal byte[] CourtOrderStatement(int idClaim, int idOrder)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_claim", idClaim },
                { "id_order", idOrder }
            };
            var fileName = "registry\\claims\\judicial_order";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        internal byte[] ExportClaims(List<int> idClaims)
        {
            string columnHeaders;
            string columnPatterns;

            columnHeaders = "[{\"columnHeader\":\"№\"},{\"columnHeader\":\"Лицевой счет\"},{\"columnHeader\":\"Адрес\"},"+
                "{\"columnHeader\":\"Наниматель\"},{\"columnHeader\":\"Дата формирования\"},{\"columnHeader\":\"Состояние установлено\"},"+
                "{\"columnHeader\":\"Текущее состояние\"},{\"columnHeader\":\"Период с\"},{\"columnHeader\":\"Период по\"},"+
                "{\"columnHeader\":\"Сумма долга найм\"},{\"columnHeader\":\"Сумма долга ДГИ\"},{\"columnHeader\":\"Сумма долга Падун\"},"+
                "{\"columnHeader\":\"Сумма долга ПКК\"},{\"columnHeader\":\"Сумма долга пени\"},{\"columnHeader\":\"Примечание\"}]";
            columnPatterns = "[{\"columnPattern\":\"$column0$\"},{\"columnPattern\":\"$column1$\"},{\"columnPattern\":\"$column2$\"},"+
                "{\"columnPattern\":\"$column3$\"},{\"columnPattern\":\"$column4$\"},{\"columnPattern\":\"$column5$\"},{\"columnPattern\":\"$column6$\"},"+
                "{\"columnPattern\":\"$column7$\"},{\"columnPattern\":\"$column8$\"},{\"columnPattern\":\"$column9$\"},{\"columnPattern\":\"$column10$\"},"+
                "{\"columnPattern\":\"$column11$\"},{\"columnPattern\":\"$column12$\"},{\"columnPattern\":\"$column13$\"},{\"columnPattern\":\"$description$\"}]";

            var fileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(fileName))
                sw.Write(string.Format("(id_claim IN ({0}))", ClaimsIdsToString(idClaims)));

            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", fileName },
                { "type", "4"},
                { "executor", securityService.User.UserName },
                { "columnHeaders", columnHeaders },
                { "columnPatterns", columnPatterns }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\export");
            return DownloadFile(fileNameReport);
        }

        private string ClaimsIdsToString(List<int> idClaims)
        {
            return idClaims.Select(r => r.ToString()).Aggregate((v, acc) => v+","+acc);
        }
    }
}
