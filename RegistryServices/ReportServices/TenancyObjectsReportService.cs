using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using RegistryDb.Models;
using RegistryServices.ViewModel.Tenancies;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RegistryWeb.ReportServices
{
    public class TenancyObjectsReportService : ReportService
    {
        private readonly RegistryContext rc;
        private readonly SecurityService securityService;

        public TenancyObjectsReportService(RegistryContext rc, IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(config, httpContextAccessor)
        {
            this.rc = rc;
            this.securityService = securityService;
        }

        public TenancyObjectReportsVM GetViewModel()
        {
            var viewModel = new TenancyObjectReportsVM
            {
                RegionsList = new SelectList(rc.KladrRegions, "IdRegion", "Region"),
                StreetsList = new SelectList(rc.KladrStreets, "IdStreet", "StreetName"),
                RentTypesList = new SelectList(rc.RentTypes, "IdRentType", "RentTypeName"),
                TenancyReasonTypesList = new SelectList(rc.TenancyReasonTypes, "IdReasonType", "ReasonName"),
                PreparersList = new SelectList(rc.Preparers, "IdPreparer", "PreparerName"),
                LawyersList = new SelectList(rc.Lawyers, "IdLawyer", "SNP")
            };
            return viewModel;
        }

        private string getTenancyStatisticFilter(TenancyStatisticModalFilter objFilter)
        {
            var filter = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(objFilter.IdRegion))
            {
                if (!string.IsNullOrEmpty(filter.ToString()))
                    filter.Append(" AND ");
                filter.Append(string.Format(CultureInfo.InvariantCulture, "SUBSTRING(v.id_street, 1, 12) = '{0}'", objFilter.IdRegion));
            }
            if (!string.IsNullOrWhiteSpace(objFilter.IdStreet))
            {
                if (!string.IsNullOrEmpty(filter.ToString()))
                    filter.Append(" AND ");
                filter.Append(string.Format(CultureInfo.InvariantCulture, "v.id_street = '{0}'", objFilter.IdStreet));
            }
            if (!string.IsNullOrWhiteSpace(objFilter.House))
            {
                if (!string.IsNullOrEmpty(filter.ToString()))
                    filter.Append(" AND ");
                filter.Append(string.Format(CultureInfo.InvariantCulture, "v.house = '{0}'", objFilter.House));
            }
            if (!string.IsNullOrWhiteSpace(objFilter.PremisesNum))
            {
                if (!string.IsNullOrEmpty(filter.ToString()))
                    filter.Append(" AND ");
                filter.Append(string.Format(CultureInfo.InvariantCulture, "(v.premises_num = '{0}' OR v.sub_premises_num = '{0}')", objFilter.PremisesNum));
            }
            if (objFilter.IdRentType.HasValue)
            {
                if (!string.IsNullOrEmpty(filter.ToString()))
                    filter.Append(" AND ");
                filter.Append(string.Format(CultureInfo.InvariantCulture, "rt.id_rent_type = {0}", objFilter.IdRentType));
            }
            if (objFilter.IdTenancyReasonType.HasValue)
            {
                if (!string.IsNullOrEmpty(filter.ToString()))
                    filter.Append(" AND ");
                filter.Append(string.Format(CultureInfo.InvariantCulture, "tp.id_process IN (SELECT id_process FROM tenancy_reasons tr WHERE rt.id_rent_type = {0})",
                    objFilter.IdTenancyReasonType));
            }
            if (objFilter.DateRegistrationFrom.HasValue || objFilter.DateRegistrationTo.HasValue)
            {
                if (!string.IsNullOrEmpty(filter.ToString()))
                    filter.Append(" AND ");
                filter.Append(filterByDateRange(objFilter.DateRegistrationFrom, objFilter.DateRegistrationTo, "registration_date"));
            }
            if (objFilter.BeginDateFrom.HasValue || objFilter.BeginDateTo.HasValue)
            {
                if (!string.IsNullOrEmpty(filter.ToString()))
                    filter.Append(" AND ");
                filter.Append(filterByDateRange(objFilter.BeginDateFrom, objFilter.BeginDateTo, "begin_date"));
            }
            if (objFilter.EndDateFrom.HasValue || objFilter.EndDateTo.HasValue)
            {
                if (!string.IsNullOrEmpty(filter.ToString()))
                    filter.Append(" AND ");
                filter.Append(filterByDateRange(objFilter.EndDateFrom, objFilter.EndDateTo, "end_date"));
            }
            return filter.ToString();
        }

        private string filterByDateRange(DateTime? dateFrom, DateTime? dateTo, string name)
        {
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture, "tp.{0} BETWEEN STR_TO_DATE('{1}','%d.%m.%Y') AND STR_TO_DATE('{2}','%d.%m.%Y %H:%i:%S')",
                    name, dateFrom.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                    dateTo.Value.AddHours(23).AddMinutes(59).AddSeconds(59).ToString("dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture));
            }
            else if (dateFrom.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture, "tp.{0} >= STR_TO_DATE('{1}','%d.%m.%Y')",
                    name, dateFrom.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
            }
            else if (dateTo.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture, "tp.{0} <= STR_TO_DATE('{1}','%d.%m.%Y %H:%i:%S')",
                    name, dateTo.Value.AddHours(23).AddMinutes(59).AddSeconds(59).ToString("dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture));
            }
            throw new Exception("Невозможно построить фильтр для формирования статистики найма");
        }

        public byte[] GetTenancyStatistic(TenancyStatisticModalFilter objFilter)
        {
            var filter = getTenancyStatisticFilter(objFilter);
            var arguments = new Dictionary<string, object>
            {
                { "filter", string.IsNullOrEmpty(filter.Trim()) ? "1=1" : filter }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\statistic");
            return DownloadFile(fileNameReport);
        }

        public byte[] GetTenancyStatisticForCoMSReporter(TenancyStatisticModalFilter modalFilter)
        {
            var filter = getTenancyStatisticFilter(modalFilter);
            var arguments = new Dictionary<string, object>
            {
                { "filter", string.IsNullOrEmpty(filter.Trim()) ? "1=1" : filter }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\tenancy_registry_for_CoMS");
            return DownloadFile(fileNameReport);
        }

        
        public byte[] GetPayment()
        {
            var fileNameReport = GenerateReport(new Dictionary<string, object>(), "registry\\tenancy\\payment");
            return DownloadFile(fileNameReport);
        }

        private string getTenancyOrderFilter(TenancyOrderModalFilter objFilter)
        {
            if (objFilter.IdStreet == null && objFilter.House == null && objFilter.PremiseNum == null && objFilter.SubPremiseNum == null)
                return "(1=1)";
            var filter = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(objFilter.IdStreet))
            {
                if (!string.IsNullOrEmpty(filter.ToString()))
                    filter.Append(" AND ");
                filter.Append(string.Format(CultureInfo.InvariantCulture, "v.id_street = '{0}'", objFilter.IdStreet));
            }
            if (!string.IsNullOrWhiteSpace(objFilter.House))
            {
                if (!string.IsNullOrEmpty(filter.ToString()))
                    filter.Append(" AND ");
                filter.Append(string.Format(CultureInfo.InvariantCulture, "v.house = '{0}'", objFilter.House));
            }
            if (!string.IsNullOrWhiteSpace(objFilter.PremiseNum))
            {
                if (!string.IsNullOrEmpty(filter.ToString()))
                    filter.Append(" AND ");
                filter.Append(string.Format(CultureInfo.InvariantCulture, "v.premises_num = '{0}'", objFilter.PremiseNum));
            }
            if (!string.IsNullOrWhiteSpace(objFilter.SubPremiseNum))
            {
                if (!string.IsNullOrEmpty(filter.ToString()))
                    filter.Append(" AND ");
                filter.Append(string.Format(CultureInfo.InvariantCulture, "v.sub_premises_num = '{0}'", objFilter.SubPremiseNum));
            }
            var result = filter.ToString();
            return string.IsNullOrEmpty(result) ? "(1=0)" : result;
        }

        private string getCourt(int? idCourt)
        {
            switch (idCourt)
            {
                /*
                case 1:
                    return "По свободным спискам(изменения)";
                case 2:
                    return "Перераспределение";
                case 3:
                    return "Основное распоряжение";
                default:
                    return "";
                */
                case 1:
                    return "Братского городского суда Иркутской области";
                case 2:
                    return "Падунского районного суда Иркутской области";
                case 3:
                    return "Иркутского областного суда";
                default:
                    return "";
            }
        }

        public byte[] GetTenancyOrder(TenancyOrderModalFilter modalFilter)
        {
            var addressFilter = getTenancyOrderFilter(modalFilter);
            var arguments = new Dictionary<string, object>
            {
                {"id_rent_type", modalFilter.IdRentType ?? 0},
                {"id_preparer", modalFilter.IdPreparer ?? 0},
                {"id_lawyer", modalFilter.IdLawyer ?? 0},
                {"orphans_num", modalFilter.OrphansNum ?? ""},
                {"orphans_date", modalFilter.OrphansDate?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)},
                {"resettle_num", modalFilter.ResettleNum ?? ""},
                {"resettle_date", modalFilter.ResettleDate?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)},
                {"resettle_type", modalFilter.IdResettleType ?? 0},
                {"entry_date" , modalFilter.EntryDate?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture )},
                {"court_num", modalFilter.CourtNum ?? ""},
                {"court_date", modalFilter.CourtDate?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)},
                {"court", getCourt(modalFilter.IdCourt)},
                {"order_type", modalFilter.IdOrderType ?? 0},
                {"registration_date_from", modalFilter.RegistrationDateFrom?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)},
                {"registration_date_to", modalFilter.RegistrationDateTo?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)},
                {"order_date_from", modalFilter.OrderDateFrom?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)},
                {"show_address", addressFilter == "(1=1)" ? "0" : "1" },
                {"id_street", modalFilter.IdStreet ??""},
                {"house", modalFilter.House ?? ""},
                {"premise", modalFilter.PremiseNum ?? ""},
                {"sub_premise", modalFilter.SubPremiseNum ?? ""},
                {"address_filter", addressFilter},
                {"summary_list_date", modalFilter.SummaryListDate?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)}
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\order_web");
            return DownloadFile(fileNameReport);
        }

        public byte[] GetTenancyNotifiesList(DateTime? dateFrom, DateTime? dateTo)
        {
            var arguments = new Dictionary<string, object>
            {
                { "date_from", dateFrom.Value },
                { "date_to", dateTo.Value },
                { "notify_type", "notify_counter" }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\tenancy\\notifies_list");
            return DownloadFile(fileNameReport);
        }
    }
}
