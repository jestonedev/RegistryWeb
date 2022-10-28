using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class TenancyOrderModalFilter
    {
        public int? IdPreparer { get; set; }
        public int? IdLawyer { get; set; }
        public string IdStreet { get; set; }
        public string House { get; set; }
        public string PremiseNum { get; set; }
        public string SubPremiseNum { get; set; }
        public DateTime? OrderDateFrom { get; set; }
        public DateTime? RegistrationDateFrom { get; set; }
        public DateTime? RegistrationDateTo { get; set; }
        public int? IdRentType { get; set; }
        public int? IdOrderType { get; set; }
        public string OrphansNum { get; set; }
        public DateTime? OrphansDate { get; set; }
        public string CourtNum { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? CourtDate { get; set; }
        public int? IdCourt { get; set; }
        public string ResettleNum { get; set; }
        public DateTime? ResettleDate { get; set; }
        public int? IdResettleType { get; set; }
        public DateTime? SummaryListDate { get; set; }
    }
}
