using RegistryWeb.Models;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.DataServices
{
    public class PrivatizationDataService : ListDataService<PrivatizationListVM, PrivatizationFilter>
    {
        public PrivatizationDataService(RegistryContext registryContext, AddressesDataService addressesDataService)
            : base(registryContext, addressesDataService)
        {
        }

        public override PrivatizationListVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, PrivatizationFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            return viewModel;
        }
    }
}