using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public class TenancyRentPeriod
    {
        public int IdRentPeriod { get; set; }
        public int IdProcess { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool UntilDismissal { get; set; }
        public byte Deleted { get; set; }

        public virtual TenancyProcess IdProcessNavigation { get; set; }
    }
}
