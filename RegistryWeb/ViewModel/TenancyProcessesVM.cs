using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class TenancyProcessesVM : ListVM<TenancyProcessesFilter>
    {
        public IEnumerable<TenancyProcess> TenancyProcesses { get; set; }
        public Dictionary<int, List<TenancyRentObject>> RentObjects { get; set; }
        public IEnumerable<TenancyReasonType> ReasonTypes { get; set; }
        public IEnumerable<RentType> RentTypes { get; set; }
        public IEnumerable<KladrStreet> Streets { get; set; }
        public IEnumerable<ObjectState> ObjectStates { get; set; }
        public IEnumerable<OwnershipRightType> OwnershipRightTypes { get; set; }
    }
}
