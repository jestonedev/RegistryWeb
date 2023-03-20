using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RegistryWeb.SecurityServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

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
            var fileName = "registry\\claims_correction\\transfer_JD";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] RequestToBks(List<int> idClaims, int idSigner, DateTime dateValue, int idReportBKSType)
        {
            var tmpFileName = Path.GetTempFileName();
            var fileconfigname = "";
            var idClaimsStr = idClaims.Select(id => id.ToString()).Aggregate((x, y) => x + "," + y);
            using (var sw = new StreamWriter(tmpFileName))
                sw.Write(idClaimsStr);

            switch (idReportBKSType)
            {
                case 1:
                    fileconfigname = "request_BKS";
                    break;
                case 2:
                    fileconfigname = "request_BKS_with_period";
                    break;
            }

            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", tmpFileName },
                { "request_date_from", dateValue.ToString("dd.MM.yyyy") },
                { "signer", idSigner },
                { "executor", securityService.Executor?.ExecutorLogin?.Split("\\")[1] }
            };

            var fileName = "registry\\claims_correction\\" + fileconfigname;
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] CourtOrderStatement(int idClaim, int idOrder)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_claim", idClaim },
                { "id_order", idOrder }
            };
            var fileName = "registry\\claims_correction\\judicial_order";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ExportClaims(List<int> idClaims)
        {
            string columnHeaders;
            string columnPatterns;

            columnHeaders = "[{\"columnHeader\":\"№ п/п\"},{\"columnHeader\":\"№\"},{\"columnHeader\":\"Лицевой счет\"},{\"columnHeader\":\"Адрес\"}," +
                "{\"columnHeader\":\"Наниматель\"},{\"columnHeader\":\"Дата формирования\"},{\"columnHeader\":\"Состояние установлено\"},"+
                "{\"columnHeader\":\"Текущее состояние\"},{\"columnHeader\":\"Период с\"},{\"columnHeader\":\"Период по\"},{\"columnHeader\":\"Номер с/п\"}," +
                "{\"columnHeader\":\"Сумма долга итого\"},{\"columnHeader\":\"Сумма долга найм\"},{\"columnHeader\":\"Сумма долга ДГИ\"},{\"columnHeader\":\"Сумма долга Падун\"}," +
                "{\"columnHeader\":\"Сумма долга ПКК\"},{\"columnHeader\":\"Сумма долга пени\"},{\"columnHeader\":\"Примечание\"}]";
            columnPatterns = "[{\"columnPattern\":\"$num$\"},{\"columnPattern\":\"$column0$\"},{\"columnPattern\":\"$column1$\"},{\"columnPattern\":\"$column2$\"}," +
                "{\"columnPattern\":\"$column3$\"},{\"columnPattern\":\"$column4$\"},{\"columnPattern\":\"$column5$\"},{\"columnPattern\":\"$column6$\"},"+
                "{\"columnPattern\":\"$column7$\"},{\"columnPattern\":\"$column8$\"},{\"columnPattern\":\"$column9$\"},{\"columnPattern\":\"$column10$\"}," +
                "{\"columnPattern\":\"$column11$\"},{\"columnPattern\":\"$column12$\"}," +
                "{\"columnPattern\":\"$column13$\"},{\"columnPattern\":\"$column14$\"},{\"columnPattern\":\"$column15$\"},{\"columnPattern\":\"$description$\"}]";

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
            var fileNameReport = GenerateReport(arguments, "registry\\claims_correction\\export");
            return DownloadFile(fileNameReport);
        }

        private string ClaimsIdsToString(List<int> idClaims)
        {
            return idClaims.Select(r => r.ToString()).Aggregate((v, acc) => v+","+acc);
        }

        public byte[] SplitAccountsReport()
        {
            var arguments = new Dictionary<string, object>();
            var fileName = "registry\\claims_correction\\payment_accounts_duplicate_statistic";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimExecutorReport(DateTime startDate, DateTime endDate, string executor)
        {
            var arguments = new Dictionary<string, object> {
                { "date_from", startDate.ToString("dd.MM.yyyy") },
                { "date_to", endDate.ToString("dd.MM.yyyy") },
                { "implementer", executor }
            };
            var fileName = "registry\\claims_correction\\claim_states";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimStatesReport(DateTime startDate, DateTime endDate, int idStateType, bool isCurrentState)        {
            var arguments = new Dictionary<string, object> {
                { "date_from", startDate.ToString("dd.MM.yyyy") },
                { "date_to", endDate.ToString("dd.MM.yyyy") },
                { "claim_state_type", idStateType },
                { "only_current_claim_state", isCurrentState ? "1" : "0" }
            };
            var fileName = "registry\\claims_correction\\claim_states_executors";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimStatesAllDatesReport(DateTime startDate, DateTime endDate, int idStateType, bool isCurrentState)
        {
            var arguments = new Dictionary<string, object> {
                { "date_from", startDate.ToString("dd.MM.yyyy") },
                { "date_to", endDate.ToString("dd.MM.yyyy") },
                { "claim_state_type", idStateType },
                { "only_current_claim_state", isCurrentState ? "1" : "0" }
            };
            var fileName = "registry\\claims_correction\\claim_states_all_dates";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimCourtReport(DateTime startDate, DateTime endDate)        {
            var arguments = new Dictionary<string, object> {
                { "date_from", startDate.ToString("dd.MM.yyyy") },
                { "date_to", endDate.ToString("dd.MM.yyyy") }
            };
            var fileName = "registry\\claims_correction\\claim_сcourt_orders_prepare";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimEmergencyTariffReport(DateTime startDate, DateTime endDate)
        {
            var arguments = new Dictionary<string, object> {
                { "date_from", startDate.ToString("dd.MM.yyyy") },
                { "date_to", endDate.ToString("dd.MM.yyyy") }
            };
            var fileName = "registry\\claims_correction\\tariffs";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimFactMailingReport(int flag, DateTime startDate, DateTime endDate)
        {
            var resultcodes = new List<int>() { -9, -8, -7, -6, -5, -4, -3, -2, -1, 0 };
            switch (flag)
            {
                case 1:
                    resultcodes = resultcodes.Where(r => r == 0).ToList();
                    break;
                case 2:
                    resultcodes = resultcodes.Where(r => r != 0).ToList();
                    break;
            }

            endDate = endDate != null ? endDate : DateTime.Now;

            var fileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(fileName))
                sw.Write(ResultCodesIdsToString(resultcodes));

            var arguments = new Dictionary<string, object> {
                { "filterTmpFile", fileName },
                { "date_from", startDate.ToString("yyyy-MM-dd") },
                { "date_to", endDate.ToString("yyyy-MM-dd") }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\claims_correction\\claim_fact_mailing");
            return DownloadFile(fileNameReport);
        }

        private string ResultCodesIdsToString(List<int> resultcodes)
        {
            return resultcodes.Aggregate("", (current, id) => current + id.ToString(CultureInfo.InvariantCulture) + ",").TrimEnd(',');
        }

        public byte[] ClaimCourtOspReport(int idClaim, DateTime createDate, int personsCount)
        {
            var fileconfigname = "";

            switch (personsCount)
            {
                case  0:
                    fileconfigname = "statement_in_osp_with_uin";
                    break;
                default:
                    fileconfigname = "statement_in_osp_with_uinAllFamily";
                    break;
            }

            var arguments = new Dictionary<string, object>
            {
                { "id_claim", idClaim },
                { "create_date", createDate },
            };

            var fileName = "registry\\claims\\" + fileconfigname;
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }
        private string getCourt(int? idCourt)
        {
            switch (idCourt)
            {
                case 1:
                    return "Падунское ОСП ГУ ФССП по Иркутской области";
                case 2:
                    return "ОСП по г. Братску и Братскому району ГУ ФССП по Иркутской области";
                default:
                    return "";
            }
        }
        public byte[] ClaimCourtSpiReport(int idClaim, int idCourtType)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_claim", idClaim },
                { "id_court_type",getCourt(idCourtType)},
                { "executor", securityService.User.UserName.Replace("PWR\\", "") }
            };
            var fileName = "registry\\claims\\statement_in_spi";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimsForDoverie(List<int> idClaims, int statusSending)
        {
            var tmpFileName = Path.GetTempFileName();
            var idClaimsStr = idClaims.Select(id => id.ToString()).Aggregate((x, y) => x + "," + y);
            using (var sw = new StreamWriter(tmpFileName))
                sw.Write(idClaimsStr);
            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", tmpFileName },
                { "statusSending", statusSending}
            };
            var fileName = "registry\\claims\\claim_statements_for_doverie_with_status";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimDateReferralCcoBailiffs(DateTime startDate, DateTime endDate)
        {
            var arguments = new Dictionary<string, object> {
                { "date_from", startDate.ToString("dd.MM.yyyy") },
                { "date_to", endDate.ToString("dd.MM.yyyy") }
            };
            var fileName = "registry\\claims_correction\\date_referral_cco_bailiffs";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimExecutedWork(DateTime startDate, DateTime endDate)
        {
            var arguments = new Dictionary<string, object> {
                { "from_date", startDate.ToString("dd.MM.yyyy") },
                { "to_date", endDate.ToString("dd.MM.yyyy") }
            };
            var fileName = "registry\\claims\\cnt_statistic_claim";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] UkInvoiceAgg(List<int> idsOrganization)
        {
            var arguments = new Dictionary<string, object> {
                { "ids", idsOrganization.Aggregate("", (acc, v) => acc+","+v.ToString() ) }
            };
            var fileName = "registry\\claims\\uk_invoice_agg";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] UkInvoiceDetails(List<int> idsOrganization)
        {
            var arguments = new Dictionary<string, object> {
                { "ids", idsOrganization.Aggregate("", (acc, v) => acc+","+v.ToString() ) }
            };
            var fileName = "registry\\claims\\uk_invoice_details";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }
    }
}
