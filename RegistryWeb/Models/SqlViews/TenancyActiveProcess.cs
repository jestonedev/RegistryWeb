using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.SqlViews
{
    public class TenancyActiveProcess
    {
        public int IdProcess { get; set; }
        public int? IdBuilding { get; set; }
        public int? IdPremises { get; set; }
        public int? IdSubPremises { get; set; }
        public string Tenants { get; set; }
        public int CountTenants { get; set; }

        public virtual TenancyProcess TenancyProcessNavigation { get; set; }
    }
}
