using RegistryWeb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class OwnerProcessesVM : ListVM<OwnerProcessesFilter>
    {
        public IEnumerable<OwnerProcess> OwnerProcesses { get; set; }
        public Dictionary<int, IEnumerable<string>> Addresses { get; set; }
    }
}
