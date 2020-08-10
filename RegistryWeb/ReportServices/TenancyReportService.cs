using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RegistryWeb.SecurityServices;
using System;
using System.Collections.Generic;
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

        public byte[] Agreement(int idAgreement)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_agreement", idAgreement }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\agreement");
            return DownloadFile(fileNameReport);
        }
    }
}
