using RegistryWeb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class OwnerProcessesListVM : ListVM<OwnerProcessesListFilter>
    {
        public IEnumerable<OwnerProcesses> OwnerProcesses { get; set; }
        public IEnumerable<OwnerType> OwnerTypes { get; set; }
    }
}
