using RegistryWeb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class OwnerProcessesVM : ListVM<OwnerProcessesFilter>
    {
        public IEnumerable<OwnerProcess> OwnerProcesses { get; set; }
        public Dictionary<int, IEnumerable<string>> Addresses { get; set; }
    }
}
