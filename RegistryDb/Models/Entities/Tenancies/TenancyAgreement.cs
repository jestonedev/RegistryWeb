using RegistryDb.Models.Entities.Common;
using System;

namespace RegistryDb.Models.Entities.Tenancies
{
    public class TenancyAgreement
    {
        public int IdAgreement { get; set; }
        public int IdProcess { get; set; }
        public DateTime? AgreementDate { get; set; }
        public string AgreementContent { get; set; }
        public int? IdExecutor { get; set; }
        public int? IdWarrant { get; set; }
        public byte Deleted { get; set; }

        public virtual TenancyProcess IdProcessNavigation { get; set; }
        public virtual Executor IdExecutorNavigation { get; set; }
    }
}
