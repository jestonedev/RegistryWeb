using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class PeriodListResult
    {
        public PeriodListResult()
        {
            GetReportingPeriodListResult = new List<ReportingPeriod>();
        }

        
        [XmlArrayItem("item")]
        public List<ReportingPeriod> GetReportingPeriodListResult { get; set; }
    }
}