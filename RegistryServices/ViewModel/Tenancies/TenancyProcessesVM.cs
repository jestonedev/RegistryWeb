using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Common;
using RegistryDb.Models.Entities.RegistryObjects.Common.Ownerships;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryDb.Models.Entities.Tenancies;
using RegistryDb.Models.SqlViews;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.Tenancies
{
    public class TenancyProcessesVM : ListVM<TenancyProcessesFilter>
    {
        public IEnumerable<TenancyProcess> TenancyProcesses { get; set; }
        public Dictionary<int, List<TenancyRentObject>> RentObjects { get; set; }
        public IEnumerable<TenancyReasonType> ReasonTypes { get; set; }
        public IEnumerable<RentType> RentTypes { get; set; }
        public IEnumerable<KladrRegion> Regions { get; set; }
        public IEnumerable<KladrStreet> Streets { get; set; }
        public IEnumerable<ObjectState> ObjectStates { get; set; }
        public IEnumerable<OwnershipRightType> OwnershipRightTypes { get; set; }
    }
}
