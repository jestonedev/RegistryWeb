using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class OwnerProcessesVM : ListVM<OwnerProcessesFilter>
    {
        public IEnumerable<OwnerProcess> OwnerProcesses { get; set; }
        public Dictionary<int, List<Address>> Addresses { get; set; }
        public IEnumerable<KladrStreet> KladrStreets { get; set; }
        public IEnumerable<OwnerType> OwnerTypes { get; set; }
    }
}
