using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.SqlViews
{
    public class TenancyActiveProcess
    {
        public int IdProcess { get; set; }
        public string Tenants { get; set; }
        public int CountTenants { get; set; }

        public virtual TenancyProcess TenancyProcessNavigation { get; set; }
    }
}
