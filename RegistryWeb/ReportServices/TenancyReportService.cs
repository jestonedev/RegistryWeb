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
    public class TenancyReportService : ReportService
    {
        private readonly SecurityService securityService;

        public TenancyReportService(IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(config, httpContextAccessor)
        {
            this.securityService = securityService;
        }

        public byte[] PreContract(int idProcess, int idPreamble, int idCommittee)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_process", idProcess },
                { "id_preamble", idPreamble },
                { "id_commitee", idCommittee }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\district_committee_pre_contract");
            return DownloadFile(fileNameReport);
        }

        public byte[] DksrContract(int idProcess)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_process", idProcess }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\contract_dksr");
            return DownloadFile(fileNameReport);
        }

        public byte[] StatementResettleSecondary(int idProcess)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_process", idProcess }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\statement_resettle_secondary");
            return DownloadFile(fileNameReport);
        }

        public byte[] Contract(int idProcess, int idRentType, int contractType, bool openDate)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_process", idProcess },
                { "opened_date", openDate ? "1" : "0" }
            };
            var fileName = "";
            switch(idRentType)
            {
                case 1:
                    fileName = "registry\\tenancy\\contract_commercial";
                    break;
                case 2:
                    if (contractType == 1)
                        fileName = "registry\\tenancy\\contract_special_1711";
                    else
                        fileName = "registry\\tenancy\\contract_special_1712";
                    break;
                case 3:
                    fileName = "registry\\tenancy\\contract_social";
                    break;
                case 4:
                    fileName = "registry\\tenancy\\contract_manevr";
                    break;
                default:
                    throw new Exception("Неизвестный тип найма");
            }
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] Act(int idProcess, bool openDate, int actType)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_process", idProcess },
                { "opened_date", openDate ? "1" : "0" },
                { "act_type", actType }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\act");
            return DownloadFile(fileNameReport);
        }



        public byte[] ActAf(int idProcess, int idPreparer)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_process", idProcess },
                { "id_preparer", idPreparer },
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\act_af");
            return DownloadFile(fileNameReport);
        }

        public byte[] Agreement(int idAgreement)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_agreement", idAgreement }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\agreement");
            return DownloadFile(fileNameReport);
        }

        public byte[] AgreementReady(int idProcess)
        {
            var arguments = new Dictionary<string, object>
            {
                {"id_process", idProcess},
                {"report_type", "2"}
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\notify_single_document");
            return DownloadFile(fileNameReport);
        }

        public byte[] NotifySingleDocument(int idProcess, int reportType)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_process", idProcess },
                { "report_type", reportType }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\notify_single_document");
            return DownloadFile(fileNameReport);
        }

        public byte[] RequestToMvd(List<int> ids, int requestType)
        {
            var tmpFileName = Path.GetTempFileName();
            var tenacyProcessesStr = ids.Select(id => id.ToString()).Aggregate((x, y) => x + "," + y);
            using (var sw = new StreamWriter(tmpFileName))
                sw.Write(tenacyProcessesStr);
            var arguments = new Dictionary<string, object>
            {
                { "idsTmpFile", tmpFileName }
            };
            var fileName = "registry\\tenancy\\requestMVD";
            if (requestType == 2)
            {
                fileName = "registry\\tenancy\\requestMVDNew";
            }
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] Notifies(List<int> ids, TenancyNotifiesReportTypeEnum reportType)
        {
            var tenacyProcessesStr = ids.Select(id => id.ToString()).Aggregate((x,y) => x + "," + y);
            var idExecutor = securityService.Executor?.IdExecutor.ToString() ?? "0";
            var arguments = new Dictionary<string, object>
            {
                { "id_executor", idExecutor },
                { "report_type", ((int)reportType).ToString() },
                { "process_ids", tenacyProcessesStr }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\notifies");
            return DownloadFile(fileNameReport);
        }

        public byte[] TenancyWarning(List<int> ids, int idPreparer, bool isMultipageDocument)
        {
            var tenacyProcessesStr = ids.Select(id => id.ToString()).Aggregate((x, y) => x + "," + y);
            var tempFileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(tempFileName))
                sw.Write(tenacyProcessesStr);
            var arguments = new Dictionary<string, object>
            {
                { "idPreparer", idPreparer.ToString() },
                { "idsTmpFile", tempFileName }
            };
            string fileNameReport = "";
            if (isMultipageDocument)
            {
                fileNameReport = GenerateReport(arguments, "registry\\tenancy\\tenancy_warning_multipage_web");
                return DownloadFile(fileNameReport);
            }
            fileNameReport = GenerateMultiFileReport(arguments, "registry\\tenancy\\tenancy_warning_web");
            return DownloadFile(fileNameReport);
        }

        public byte[] ExportReasonsForGisZkh(List<int> ids)
        {
            var tenacyProcessesStr = ids.Select(id => id.ToString()).Aggregate((x, y) => x + "," + y);
            var tempFileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(tempFileName))
                sw.Write(tenacyProcessesStr);
            var arguments = new Dictionary<string, object>
            {
                { "idsTmpFile", tempFileName }
            };
            var fileNameReport = GenerateMultiFileReport(arguments, "registry\\tenancy\\gis_zkh_rent_reason_web");
            return DownloadFile(fileNameReport);
        }

        public byte[] GisZkhExport(List<int> ids)
        {
            var tenacyProcessesStr = ids.Select(id => id.ToString()).Aggregate((x, y) => x + "," + y);
            var filter = string.Format("tp.id_process IN ({0}) AND ", tenacyProcessesStr);
            var tempFileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(tempFileName))
                sw.Write(filter);
            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", tempFileName }

            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\gis_zkh_export");
            return DownloadFile(fileNameReport);
        }

        internal byte[] TenanciesExport(List<int> idProcesses)
        {
            string columnHeaders;
            string columnPatterns;

            columnHeaders = "[{\"columnHeader\":\"№\"},{\"columnHeader\":\"№ договора\"},{\"columnHeader\":\"Дата регистрации договора\"},"+
                "{\"columnHeader\":\"Дата окончания договора\"},{\"columnHeader\":\"№ ордера / другого документа-основания\"},"+
                "{\"columnHeader\":\"Наниматель\"},{\"columnHeader\":\"Тип найма\"},{\"columnHeader\":\"Нанимаемое жилье\"},"+
                "{\"columnHeader\":\"Дополнительные сведения\"}]";
            columnPatterns = "[{\"columnPattern\":\"$column0$\"},{\"columnPattern\":\"$column1$\"},{\"columnPattern\":\"$column2$\"},"+
                "{\"columnPattern\":\"$column3$\"},{\"columnPattern\":\"$column4$\"},{\"columnPattern\":\"$column5$\"},"+
                "{\"columnPattern\":\"$column6$\"},{\"columnPattern\":\"$column7$\"},{\"columnPattern\":\"$description$\"}]";

            var fileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(fileName))
                sw.Write(string.Format("(id_process IN ({0}))", ProcessesIdsToString(idProcesses)));

            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", fileName },
                { "type", "3"},
                { "executor", securityService.User.UserName },
                { "columnHeaders", columnHeaders },
                { "columnPatterns", columnPatterns }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\export");
            return DownloadFile(fileNameReport);
        }

        private string ProcessesIdsToString(List<int> idProcesses)
        {
            return idProcesses.Select(r => r.ToString()).Aggregate((v, acc) => v + "," + acc);
        }
    }
}
