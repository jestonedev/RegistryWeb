using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.Privatization;
using RegistryDb.Models.Entities.Tenancies;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.Common
{
    public partial class Executor
    {
        public int IdExecutor { get; set; }
        public string ExecutorPost { get; set; }
        public string ExecutorName { get; set; }
        public string ExecutorLogin { get; set; }
        public string Phone { get; set; }
        public bool IsInactive { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<TenancyProcess> TenancyProcesses { get; set; }
        public virtual IList<TenancyAgreement> TenancyAgreements { get; set; }
        public virtual IList<ClaimCourtOrder> ClaimCourtOrders { get; set; }
        public virtual IList<PrivContract> PrivContracts { get; set; }
    }
}
