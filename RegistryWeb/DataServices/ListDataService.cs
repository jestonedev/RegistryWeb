using RegistryWeb.Models;
using RegistryWeb.ViewOptions;

namespace RegistryWeb.DataServices
{
    public class ListDataService<IVM, F> : IListDataService
        where IVM: ViewModel.ListVM<F>, new()
        where F: FilterOptions, new()
    {
        protected readonly RegistryContext registryContext;

        public ListDataService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
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
