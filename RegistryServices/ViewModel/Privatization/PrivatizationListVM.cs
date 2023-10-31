using RegistryDb.Models.Entities.Privatization;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.Privatization
{
    public class PrivatizationListVM : ListVM<PrivatizationFilter>
    {
        public List<PrivContract> PrivContracts { get; set; }
        public Dictionary<int, List<Address>> PrivContractsAddresses { get; set; }
    }
}