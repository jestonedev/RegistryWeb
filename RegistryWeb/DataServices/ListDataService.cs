using RegistryWeb.Models;
using RegistryWeb.ViewOptions;

namespace RegistryWeb.DataServices
{
    public class ListDataService<IVM, F> : IListDataService
        where IVM: ViewModel.ListVM<F>, new()
        where F: FilterOptions, new()
    {
        protected readonly RegistryContext registryContext;
        protected readonly AddressesDataService addressesDataService;

        public ListDataService(RegistryContext registryContext, AddressesDataService addressesDataService)
        {
            this.registryContext = registryContext;
            this.addressesDataService = addressesDataService;
        }

        public virtual IVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, F filterOptions)
            => new IVM
            {
                OrderOptions = orderOptions ?? new OrderOptions(),
                PageOptions = pageOptions ?? new PageOptions(),
                FilterOptions = filterOptions ?? new F()
            };
    }
}
