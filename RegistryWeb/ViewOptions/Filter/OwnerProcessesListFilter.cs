using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class OwnerProcessesListFilter : FilterOptions
    {
        public string Street { get; set; }
        public int? IdProcessType { get; set; }
    }
}
