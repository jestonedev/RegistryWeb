using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class Executor
    {
        public int IdExecutor { get; set; }
        public string ExecutorName { get; set; }
        public string ExecutorLogin { get; set; }
        public string Phone { get; set; }
        public bool IsInactive { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<TenancyProcess> TenancyProcesses { get; set; }
        public virtual IList<TenancyAgreement> TenancyAgreements { get; set; }
    }
}
