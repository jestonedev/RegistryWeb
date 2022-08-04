using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public class TenancyAgreement
    {
        public int IdAgreement { get; set; }
        public int IdProcess { get; set; }
        public DateTime? AgreementDate { get; set; }
        public string AgreementContent { get; set; }
        public DateTime? IssuedDate { get; set; }
        public int? IdExecutor { get; set; }
        public int? IdWarrant { get; set; }
        public byte Deleted { get; set; }

        public virtual TenancyProcess IdProcessNavigation { get; set; }
        public virtual Executor IdExecutorNavigation { get; set; }
    }
}
