using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class OwnerProcessesFilter : FilterOptions
    {
        public OwnerProcessesFilter()
        {
            Address = new Address();
        }

        public Address Address { get; set; }
        public int? IdOwnerType { get; set; }
        public int? IdProcess { get; set; }
    }
}
