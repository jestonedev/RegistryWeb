using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class TenancyStatisticModalFilter
    {
        public int IdReportType { get; set; }
        public string IdRegion { get; set; }
        public string IdStreet { get; set; }
        public string House { get; set; }
        public string PremisesNum { get; set; }
        public int? IdRentType { get; set; }
        public int? IdTenancyReasonType { get; set; }
        public DateTime? DateRegistrationFrom { get; set; }
        public DateTime? DateRegistrationTo { get; set; }
        public DateTime? BeginDateFrom { get; set; }
        public DateTime? BeginDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
    }
}
