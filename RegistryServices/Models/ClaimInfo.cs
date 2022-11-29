using System;

namespace RegistryWeb.ViewModel
{
    public class ClaimInfo
    {
        public int IdClaim { get; set; }
        public int IdAccount { get; set; }
        public int? IdClaimCurrentState { get; set; }
        public string ClaimCurrentState { get; set; }
        public DateTime? ClaimCurrentStateDate { get; set; }
        public DateTime? StartDeptPeriod { get; set; }
        public DateTime? EndDeptPeriod { get; set; }
        public string ClaimDescription { get; set; }
        public bool EndedForFilter { get; set; }
        public decimal? AmountTenancy { get; set; }
        public decimal? AmountPenalty { get; set; }
    }
}
