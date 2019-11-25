using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public class TenancyActiveProcess
    {
        public int IdProcess { get; set; }
        public string Tenants { get; set; }

        public virtual TenancyProcess TenancyProcessNavigation { get; set; }
    }
}
