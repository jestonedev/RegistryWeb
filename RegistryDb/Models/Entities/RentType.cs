using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public partial class RentType
    {
        public RentType()
        {
            RentTypeCategories = new List<RentTypeCategory>();
            TenancyProcesses = new List<TenancyProcess>();
        }

        public int IdRentType { get; set; }
        public string RentTypeName { get; set; }
        public string RentTypeShort { get; set; }
        public string RentTypeGenetive { get; set; }

        public virtual IList<RentTypeCategory> RentTypeCategories { get; set; }
        public IList<TenancyProcess> TenancyProcesses { get; set; }
    }
}
