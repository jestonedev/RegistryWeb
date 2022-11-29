using RegistryWeb.Enums;
using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewOptions
{
    public class PrivCommonReportSettings
    {
        public PrivReportTypeEnum ReportType { get; set; }
        public string ReportName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? RegEgrpStartDate { get; set; }
        public DateTime? RegEgrpEndDate { get; set; }
        public DateTime? InsertStartDate { get; set; }
        public DateTime? InsertEndDate { get; set; }
        public DateTime? RegContractStartDate { get; set; }
        public DateTime? RegContractEndDate { get; set; }
        public DateTime? IssueCivilStartDate { get; set; }
        public DateTime? IssueCivilEndDate { get; set; }
        public List<string> IdRegion { get; set; }
        public string IdStreet { get; set; }
        public string House { get; set; }
        public PrivEstateTypeEnum? EstateType { get; set; }
        public PrivLiteraTypeEnum? LiteraType { get; set; }
        public int? IdExecutor { get; set; }
        public PrivOrderEnum? Order { get; set; }
    }
}
