using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class BuildingsListFilter : FilterOptions
    {
        public string Street { get; set; }
        public int? IdObjectState { get; set; }
    }
}
