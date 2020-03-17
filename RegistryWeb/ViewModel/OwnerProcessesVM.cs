using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class OwnerProcessesVM : ListVM<OwnerProcessesFilter>
    {
        public IEnumerable<OwnerProcess> OwnerProcesses { get; set; }
        public Dictionary<int, List<Address>> Addresses { get; set; }
    }
}
