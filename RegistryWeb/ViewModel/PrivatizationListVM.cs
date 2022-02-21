using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class PrivatizationListVM : ListVM<PrivatizationFilter>
    {
        public List<PrivContract> PrivContracts { get; set; }
        public Dictionary<int, List<Address>> PrivContractsAddresses { get; set; }
    }
}