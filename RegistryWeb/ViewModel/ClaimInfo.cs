using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class ClaimInfo
    {
        public int IdClaim { get; set; }
        public int IdAccount { get; set; }
        public int? IdClaimCurrentState { get; set; }
        public string ClaimCurrentState { get; set; }
        public DateTime? StartDeptPeriod { get; set; }
        public DateTime? EndDeptPeriod { get; set; }
        public bool EndedForFilter { get; set; }
    }
}
