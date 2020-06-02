using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class TenancyProcessesVM : ListVM<TenancyProcessesFilter>
    {
        public IEnumerable<TenancyProcess> TenancyProcesses { get; set; }
        public Dictionary<int, List<Address>> Addresses { get; set; }
    }
}
