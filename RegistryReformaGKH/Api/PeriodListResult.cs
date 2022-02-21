using System.Collections.Generic;
using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
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