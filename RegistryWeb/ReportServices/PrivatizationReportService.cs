using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ReportServices
{
    public class PrivatizationReportService : ReportService
    {
        private readonly SecurityService securityService;

        public PrivatizationReportService(IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(config, httpContextAccessor)
        {
            this.securityService = securityService;
        }

        internal byte[] GetCommonReport(PrivCommonReportSettings settings)
        {
            var reportConfig = GetCommonReportConfigFile(settings);
            var arguments = BuildCommonReportArguments(settings);
            var fileNameReport = GenerateReport(arguments, "registry\\privatization\\" + reportConfig);
            return DownloadFile(fileNameReport);
        }

        private string RangeDateToWhereClause(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null) startDate = new DateTime(1991, 1, 1);
            if (endDate == null) endDate = new DateTime(2100, 12, 31);
            return string.Format("BETWEEN STR_TO_DATE('{0}','%d.%m.%Y') AND STR_TO_DATE('{1}','%d.%m.%Y')",
                startDate.Value.ToString("dd.MM.yyyy"), endDate.Value.ToString("dd.MM.yyyy"));
        }

        private Dictionary<string, object> BuildCommonReportArguments(PrivCommonReportSettings settings)
        {
            var arguments = new Dictionary<string, object>();
            var where = "";

            switch (settings.ReportType)
            {
                case PrivReportTypeEnum.StatByFirstPrivatization: // +
                    where = "(1=1)";
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND vpei.id_region IN('0'";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(",'{0}'", IdRegion);
                        }
                        where += ")";
                    }
                    if (!string.IsNullOrEmpty(settings.IdStreet))
                    {
                        where += string.Format(" AND vpei.id_street = '{0}'", settings.IdStreet);
                    }
                    if (!string.IsNullOrEmpty(settings.House))
                    {
                        where += string.Format(" AND vpei.house = '{0}'", settings.House);
                    }
                    break;
                case PrivReportTypeEnum.NoEgrpReg: // +
                    where = "date_issue " + RangeDateToWhereClause(settings.StartDate, settings.EndDate);
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND id_region IN('0'";
                        foreach(var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(",'{0}'", IdRegion);
                        }
                        where += ")";
                    }
                    arguments.Add("param_date1", settings.StartDate.HasValue ? settings.StartDate.Value.ToString("dd.MM.yyyy") : "");
                    arguments.Add("param_date2", settings.EndDate.HasValue ? settings.EndDate.Value.ToString("dd.MM.yyyy") : "");
                    break;
                case PrivReportTypeEnum.ForProcuracy: // +
                    where = "date_issue " + RangeDateToWhereClause(settings.StartDate, settings.EndDate);
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND id_region IN('0'";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(",'{0}'", IdRegion);
                        }
                        where += ")";
                    }
                    if (!string.IsNullOrEmpty(settings.IdStreet))
                    {
                        where += string.Format(" AND vpei.id_street = '{0}'", settings.IdStreet);
                    }
                    if (!string.IsNullOrEmpty(settings.House))
                    {
                        where += string.Format(" AND vpei.house = '{0}'", settings.House);
                    }
                    break;
                case PrivReportTypeEnum.PrivEstateWithAreaDetails: // +
                    where = "date_issue " + RangeDateToWhereClause(settings.StartDate, settings.EndDate);
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND id_region IN('0'";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(",'{0}'", IdRegion);
                        }
                        where += ")";
                    }
                    if (!string.IsNullOrEmpty(settings.IdStreet))
                    {
                        where += string.Format(" AND vpei.id_street = '{0}'", settings.IdStreet);
                    }
                    if (settings.EstateType != null)
                    {
                        where += string.Format(" AND estate_type = {0}", (int)settings.EstateType);
                    }
                    break;
                case PrivReportTypeEnum.PrivSubPremises: // +
                case PrivReportTypeEnum.PrivPremises: // +
                case PrivReportTypeEnum.PrivHouses: // +
                case PrivReportTypeEnum.PrivRoomsInPremiseWithSettle: // +
                case PrivReportTypeEnum.Unprivatization: // +
                    where = "date_issue " + RangeDateToWhereClause(settings.StartDate, settings.EndDate);
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND id_region IN('0'";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(",'{0}'", IdRegion);
                        }
                        where += ")";
                    }
                    if (!string.IsNullOrEmpty(settings.IdStreet))
                    {
                        where += string.Format(" AND vpei.id_street = '{0}'", settings.IdStreet);
                    }
                    if (!string.IsNullOrEmpty(settings.House))
                    {
                        where += string.Format(" AND vpei.house = '{0}'", settings.House);
                    }
                    arguments.Add("param_date1", settings.StartDate.HasValue ? settings.StartDate.Value.ToString("dd.MM.yyyy") : "");
                    arguments.Add("param_date2", settings.EndDate.HasValue ? settings.EndDate.Value.ToString("dd.MM.yyyy") : "");
                    break;
                case PrivReportTypeEnum.ByAddress: // +
                    where = "(1=1)";
                    if (!string.IsNullOrEmpty(settings.IdStreet))
                    {
                        where += string.Format(" AND vpei.id_street = '{0}'", settings.IdStreet);
                    }
                    if (!string.IsNullOrEmpty(settings.House))
                    {
                        where += string.Format(" AND vpei.house = '{0}'", settings.House);
                    }
                    break;
                case PrivReportTypeEnum.ByRegion:
                    where = "(1=1)";
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND vpei.id_region IN('0'";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(",'{0}'", IdRegion);
                        }
                        where += ")";
                    }
                    break;
                case PrivReportTypeEnum.ByApplicationDate: // +
                    where = "application_date " + RangeDateToWhereClause(settings.StartDate, settings.EndDate);
                    break;
                case PrivReportTypeEnum.OrderAdditional:
                    where = "date_issue " + RangeDateToWhereClause(settings.StartDate, settings.EndDate);
                    if (settings.RegEgrpStartDate != null && settings.RegEgrpEndDate != null)
                    {
                        where += " AND registration_date " + RangeDateToWhereClause(settings.RegEgrpStartDate, settings.RegEgrpEndDate);
                    }
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND (1=0";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(" OR id_region LIKE '%{0}%'", IdRegion);
                        }
                        where += ")";
                    }
                    arguments.Add("param_date1", settings.StartDate.HasValue ? settings.StartDate.Value.ToString("dd.MM.yyyy") : "");
                    arguments.Add("param_date2", settings.EndDate.HasValue ? settings.EndDate.Value.ToString("dd.MM.yyyy") : "");
                    arguments.Add("id_executor", settings.IdExecutor);
                    break;
                case PrivReportTypeEnum.OrderExcludePremises: // +
                    where = "date_issue " + RangeDateToWhereClause(settings.StartDate, settings.EndDate);
                    if (settings.RegEgrpStartDate != null && settings.RegEgrpEndDate != null)
                    {
                        where += " AND registration_date " + RangeDateToWhereClause(settings.RegEgrpStartDate, settings.RegEgrpEndDate);
                    }
                    if (settings.InsertStartDate != null && settings.InsertEndDate != null)
                    {
                        where += " AND insert_date " + RangeDateToWhereClause(settings.InsertStartDate, settings.InsertEndDate);
                    }
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND id_region IN('0'";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(",'{0}'", IdRegion);
                        }
                        where += ")";
                    }
                    break;
                case PrivReportTypeEnum.RegistryForCpmu:  // +
                case PrivReportTypeEnum.StatByContractors: 
                    where = "date_issue " + RangeDateToWhereClause(settings.StartDate, settings.EndDate);
                    break;
                case PrivReportTypeEnum.StatIssuedNotRegistred:  // +
                    where = "(1=1)";
                    if (settings.RegContractStartDate != null || settings.RegContractEndDate != null)
                    {
                        where += " AND date_issue " + RangeDateToWhereClause(settings.RegContractStartDate, settings.RegContractEndDate);
                    }
                    if (settings.IssueCivilStartDate != null || settings.IssueCivilEndDate != null)
                    {
                        where += " AND date_issue_civil " + RangeDateToWhereClause(settings.IssueCivilStartDate, settings.IssueCivilEndDate);
                    }
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND (1=0";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(" OR id_region LIKE '%{0}%'", IdRegion);
                        }
                        where += ")";
                    }
                    break;
                case PrivReportTypeEnum.StatByIssueDate:  // +
                    where = "(1=1)";
                    if (settings.RegContractStartDate != null || settings.RegContractEndDate != null)
                    {
                        where += " AND date_issue " + RangeDateToWhereClause(settings.RegContractStartDate, settings.RegContractEndDate);
                    }
                    if (settings.IssueCivilStartDate != null || settings.IssueCivilEndDate != null)
                    {
                        where += " AND date_issue_civil " + RangeDateToWhereClause(settings.IssueCivilStartDate, settings.IssueCivilEndDate);
                    }
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND (1=0";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(" OR id_region LIKE '%{0}%'", IdRegion);
                        }
                        where += ")";
                    }
                    if (settings.Order != null)
                    {
                        switch(settings.Order)
                        {
                            case PrivOrderEnum.ByDate:
                                where += " ORDER BY date_issue";
                                break;
                            case PrivOrderEnum.ByRegNum:
                                where += " ORDER BY reg_number";
                                break;
                        }
                    }
                    break;
                case PrivReportTypeEnum.StatByIssueCivilDate:  // +
                    where = "date_issue_civil " + RangeDateToWhereClause(settings.StartDate, settings.EndDate);
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND id_region IN('0'";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(",'{0}'", IdRegion);
                        }
                        where += ")";
                    }
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND (1=0";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(" OR id_region LIKE '%{0}%'", IdRegion);
                        }
                        where += ")";
                    }
                    break;
                case PrivReportTypeEnum.StatByRegDate:  // +
                    where = "(1=1)";
                    if (settings.StartDate != null || settings.EndDate != null)
                    {
                        where += " AND date_issue " + RangeDateToWhereClause(settings.StartDate, settings.EndDate);
                    }
                    if (settings.RegEgrpStartDate != null || settings.RegEgrpEndDate != null)
                    {
                        where += " AND registration_date " + RangeDateToWhereClause(settings.RegEgrpStartDate, settings.RegEgrpEndDate);
                    }
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND (1=0";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(" OR id_region LIKE '%{0}%'", IdRegion);
                        }
                        where += ")";
                    }
                    break;
                case PrivReportTypeEnum.StatByRegEgrpDate:  // +
                    where = "registration_date " + RangeDateToWhereClause(settings.RegEgrpStartDate, settings.RegEgrpEndDate);
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND (1=0";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(" OR id_region LIKE '%{0}%'", IdRegion);
                        }
                        where += ")";
                    }
                    break;
                case PrivReportTypeEnum.StatByWarrantPerson:
                    where = "date_issue " + RangeDateToWhereClause(settings.StartDate, settings.EndDate);
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND (1=0";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(" OR id_region LIKE '%{0}%'", IdRegion);
                        }
                        where += ")";
                    }
                    break;
                case PrivReportTypeEnum.StatByRefusenik:
                case PrivReportTypeEnum.StatByRefuse:
                    if (settings.StartDate == null && settings.EndDate == null)
                        where += "application_date IS NULL OR application_date " + RangeDateToWhereClause(settings.StartDate, settings.EndDate);
                    else
                        where += "application_date " + RangeDateToWhereClause(settings.StartDate, settings.EndDate);
                    if (settings.IdRegion != null && settings.IdRegion.Any())
                    {
                        where += " AND id_region IN('0'";
                        foreach (var IdRegion in settings.IdRegion)
                        {
                            where += string.Format(",'{0}'", IdRegion);
                        }
                        where += ")";
                    }
                    if (!string.IsNullOrEmpty(settings.IdStreet))
                    {
                        where += string.Format(" AND vpei.id_street = '{0}'", settings.IdStreet);
                    }
                    break;
                default:
                    throw new Exception("Неизвестный отчет");
            }
            if (!string.IsNullOrEmpty(where))
                arguments.Add("where_var", where);

            return arguments;
        }

        private string GetCommonReportConfigFile(PrivCommonReportSettings settings)
        {
            switch(settings.ReportType)
            {
                case PrivReportTypeEnum.StatByFirstPrivatization:
                    return "statFirstPriv";
                case PrivReportTypeEnum.NoEgrpReg:
                    return "not_rosReestr";
                case PrivReportTypeEnum.ForProcuracy:
                    return "rep_prosecutor";
                case PrivReportTypeEnum.PrivEstateWithAreaDetails:
                    return "rep_priv_S";
                case PrivReportTypeEnum.PrivRoomsInPremiseWithSettle:
                    return "stat_privatizSharesRooms";
                case PrivReportTypeEnum.PrivHouses:
                    return "stat_privatizHouses";
                case PrivReportTypeEnum.PrivPremises:
                    return "stat_privatizApart";
                case PrivReportTypeEnum.PrivSubPremises:
                    return "stat_privatizRooms";
                case PrivReportTypeEnum.Unprivatization:
                    return "stat_rasprivat";
                case PrivReportTypeEnum.ByAddress:
                    return "stat_address";
                case PrivReportTypeEnum.ByRegion:
                    return "stat_location";
                case PrivReportTypeEnum.ByApplicationDate:
                    if (settings.LiteraType == PrivLiteraTypeEnum.WithLiteraP)
                        return "stat_AppDate_p";
                    else
                        return "stat_AppDate";
                case PrivReportTypeEnum.OrderAdditional:
                    return "stat_for_accounting";
                case PrivReportTypeEnum.OrderExcludePremises:
                    return "rep_pasp";
                case PrivReportTypeEnum.RegistryForCpmu:
                    return "stat_contrdate";
                case PrivReportTypeEnum.StatIssuedNotRegistred:
                    return "stat_notReg";
                case PrivReportTypeEnum.StatByIssueDate:
                    return "stat_DoI_DoIC";
                case PrivReportTypeEnum.StatByIssueCivilDate:
                    return "stat_DoIC";
                case PrivReportTypeEnum.StatByRegDate:
                    return "stat_regDate_DoIC";
                case PrivReportTypeEnum.StatByRegEgrpDate:
                    return "stat_regDate";
                case PrivReportTypeEnum.StatByWarrantPerson:
                    return "stat_dover";
                case PrivReportTypeEnum.StatByRefusenik:
                    return "stat_refusenik";
                case PrivReportTypeEnum.StatByRefuse:
                    return "stat_refuse";
                case PrivReportTypeEnum.StatByContractors:
                    return "stat_contractors";
                default:
                    throw new Exception("Неизвестный отчет");
            }
        }

        internal byte[] GetQuarterReport(PrivQuarterReportSettings settings)
        {
            var filterField = "";
            switch(settings.FilterField)
            {
                case PrivQuarterReportFilterFieldEnum.IssueCivil:
                    filterField = "date_issue_civil";
                    break;
                case PrivQuarterReportFilterFieldEnum.RegDate:
                    filterField = "date_issue";
                    break;
                case PrivQuarterReportFilterFieldEnum.RegEgrp:
                    filterField = "registration_date";
                    break;
                case PrivQuarterReportFilterFieldEnum.RequestDate:
                    filterField = "application_date";
                    break;
            }
            var quarter = 0;
            if (settings.Quarter != null && settings.Quarter <= 4 && settings.Quarter >= 1)
            {
                quarter = settings.Quarter.Value;
            }
            var arguments = new Dictionary<string, object> {
                { "year",  settings.Year },
                { "quarter",  quarter },
                { "filter_field", filterField }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\privatization\\rep_quarterly");
            return DownloadFile(fileNameReport);
        }

        internal byte[] GetContract(PrivContractReportSettings settings)
        {
            // TODO диалог с дополнительными конфигурационными параметрами
            var arguments = new Dictionary<string, object> {
                { "id_contract", settings.IdContract },
                { "id_owner", settings.IdOwner },
                { "id_owner_signer", settings.IdOwnerSigner },
                { "contract_kind", (int)settings.ContractKind },
                { "contract_type", (int)settings.ContractType },
            };
            var fileName = "registry\\privatization\\contract";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }
    }
}
