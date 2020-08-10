using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RegistryWeb.Models;
using RegistryWeb.SecurityServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ReportServices
{
    public class PremiseReportService : ReportService
    {
        private readonly RegistryContext rc;
        private readonly SecurityService securityService;

        public PremiseReportService(RegistryContext rc, IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(config, httpContextAccessor)
        {
            this.rc = rc;
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

        public byte[] PremisesArea(string idPremises)
        {
            var fileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(fileName))
                sw.Write(idPremises);
            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", fileName },
            };
            var fileNameReport = GenerateReport(arguments, "registry\\registry\\area_premises");
            return DownloadFile(fileNameReport);
        }

        //___________для массовых______________
        public byte[] ExcerptPremises(string idPremises, string excerptNumber, DateTime excerptDateFrom, int signer)
        {
            var fileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(fileName))
                sw.Write(idPremises);
            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", fileName },
                { "excerpt_type", 4 },
                { "excerpt_date_from", excerptDateFrom.ToString("dd.MM.yyyy") },
                { "excerpt_number", excerptNumber },
                { "executor", securityService.User.UserName },
                { "signer", signer }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\registry\\premises_mx");
            return DownloadFile(fileNameReport);
        }

        public byte[] MassActPremises(string idPremises, DateTime actDate, string isNotResides, string commision, int clerk)
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
            var fileNameReport = GenerateReport(arguments, "registry\\registry\\act_residence");
            return DownloadFile(fileNameReport);
        }

        public byte[] ExportPremises(string idPremises)
        {
            string columnHeaders;
            string columnPatterns;
            if (securityService.HasPrivilege(Privileges.TenancyRead))
            {
                columnHeaders = "{\"columnHeader\":\"№ по реестру\"},{\"columnHeader\":\"Адрес\"},{\"columnHeader\":\"Дом\"}," +
                    "{\"columnHeader\":\"Пом.\"},{\"columnHeader\":\"Тип помещения\"},{\"columnHeader\":\"Кадастровый номер\"}," +
                    "{\"columnHeader\":\"Общая площадь\"},{\"columnHeader\":\"Текущее состояние\"},{\"columnHeader\":\"Текущий фонд\"}," +
                    "{\"columnHeader\":\"№ договора найма\"},{\"columnHeader\":\"Дата регистрации договора\"}," +
                    "{\"columnHeader\":\"Дата окончания договора\"},{\"columnHeader\":\"№ ордера найма\"},{\"columnHeader\":\"Дата ордера найма\"}," +
                    "{\"columnHeader\":\"Наниматель\"},{\"columnHeader\":\"Размер платы\"},{\"columnHeader\":\"Номер и дата включения в фонд\"}," +
                    "{\"columnHeader\":\"Дополнительные сведения\"}";
                columnPatterns = "{\"columnPattern\":\"$column0$\"},{\"columnPattern\":\"$column1$\"},{\"columnPattern\":\"$column2$\"}," +
                    "{\"columnPattern\":\"$column3$\"},{\"columnPattern\":\"$column4$\"},{\"columnPattern\":\"$column5$\"}," +
                    "{\"columnPattern\":\"$column6$\"},{\"columnPattern\":\"$column8$\"},{\"columnPattern\":\"$column9$\"}," +
                    "{\"columnPattern\":\"$column10$\"},{\"columnPattern\":\"$column11$\"}," +
                    "{\"columnPattern\":\"$column12$\"},{\"columnPattern\":\"$column13$\"},{\"columnPattern\":\"$column14$\"}," +
                    "{\"columnPattern\":\"$column15$\"},{\"columnPattern\":\"$column16$\"},{\"columnPattern\":\"$fund_info$\"}," +
                    "{\"columnPattern\":\"$description$\"}";
            }
            else
            {
                columnHeaders = "{\"columnHeader\":\"№ по реестру\"},{\"columnHeader\":\"Адрес\"},{\"columnHeader\":\"Дом\"}," +
                    "{\"columnHeader\":\"Пом.\"},{\"columnHeader\":\"Тип помещения\"},{\"columnHeader\":\"Кадастровый номер\"}," +
                    "{\"columnHeader\":\"Общая площадь\"},{\"columnHeader\":\"Текущее состояние\"},{\"columnHeader\":\"Текущий фонд\"}," +
                    "{\"columnHeader\":\"Дополнительные сведения\"}";
                columnPatterns = "{\"columnPattern\":\"$column0$\"},{\"columnPattern\":\"$column1$\"},{\"columnPattern\":\"$column2$\"}," +
                    "{\"columnPattern\":\"$column3$\"},{\"columnPattern\":\"$column4$\"},{\"columnPattern\":\"$column5$\"}," +
                    "{\"columnPattern\":\"$column6$\"},{\"columnPattern\":\"$column8$\"},{\"columnPattern\":\"$fund_info$\"}," +
                    "{\"columnPattern\":\"$description$\"}";
            }

            var fileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(fileName))
                sw.Write(string.Format("({0}) OR (id_premises IN ({1}))", "0 = 1", idPremises));

            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", fileName },
                { "type", "2"},
                { "executor", securityService.User.UserName },
                {
                    "columnHeaders", "["+columnHeaders+",{\"columnHeader\":\"Номер и дата включения в фонд\"},{\"columnHeader\":\"Дополнительные сведения\"}"+
                    ",{\"columnHeader\":\"Основание на включение в АФ здания\"},{\"columnHeader\":\"Основание на включение в АФ помещения\"}"+
                    ",{\"columnHeader\":\"Основание на включение здания в перечень снесенных\"},{\"columnHeader\":\"Основание на включение помещения в перечень снесенных\"}]"
                },
                {
                    "columnPatterns", "["+columnPatterns+",{\"columnPattern\":\"$fund_info$\"},{\"columnPattern\":\"$description$\"}"+
                    ",{\"columnPattern\":\"$b_emergency$\"},{\"columnPattern\":\"$p_emergency$\"}"+
                    ",{\"columnPattern\":\"$b_demolished$\"},{\"columnPattern\":\"$p_demolished$\"}]"
                }

            };
            var fileNameReport = GenerateReport(arguments, "registry\\export");
            return DownloadFile(fileNameReport);
        }

        public byte[] TenancyHistoryPremises(string idPremises)
        {
            var fileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(fileName))
                sw.Write(string.Format("({0}) OR (id_state = 1 AND id_premises IN ({1}))", "id_state IN (4,5,9,11,12)", idPremises));

            var arguments = new Dictionary<string, object>
            {{ "filterTmpFile", fileName },
                { "executor", securityService.User.UserName }

            };
            var fileNameReport = GenerateReport(arguments, "registry\\registry\\tenancy_history");
            return DownloadFile(fileNameReport);
        }


        public byte[] TotalTenancyHistoryPremises() //метод для получения Истории найма для всех муниципальных помещ-й
        {
            var states = new List<int> { 4, 5, 9, 11, 12 };
            var municipalIds = from premisesRow in rc.Premises
                               join subPremisesRow in rc.SubPremises
                               on premisesRow.IdPremises equals subPremisesRow.IdPremises into ps
                               from psRow in ps.DefaultIfEmpty()
                               where psRow != null && states.Contains(psRow.IdState)
                               select premisesRow.IdPremises;
            var ids = municipalIds
                .Aggregate("", (current, id) => current + id.ToString(CultureInfo.InvariantCulture) + ",").TrimEnd(',');
            var municipalStateIds = states
                .Aggregate("", (current, id) => current + id.ToString(CultureInfo.InvariantCulture) + ",").Trim(',');

            var arguments = new Dictionary<string, object>
            {
                { "filter", string.Format("(id_state IN ({0}) OR (id_state = 1 AND id_premises IN (0{1})))", municipalStateIds, ids) },
                { "executor", securityService.User.UserName }

            };
            var fileNameReport = GenerateReport(arguments, "registry\\registry\\tenancy_history");
            return DownloadFile(fileNameReport);
        }
    }
}
