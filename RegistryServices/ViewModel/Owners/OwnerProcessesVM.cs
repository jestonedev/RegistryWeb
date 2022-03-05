using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Owners;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.Owners
{
    public class OwnerProcessesVM : ListVM<OwnerProcessesFilter>
    {
        public IEnumerable<OwnerProcess> OwnerProcesses { get; set; }
        public Dictionary<int, List<Address>> Addresses { get; set; }
        public IEnumerable<KladrStreet> KladrStreets { get; set; }
        public IEnumerable<OwnerType> OwnerTypes { get; set; }
    }
}
