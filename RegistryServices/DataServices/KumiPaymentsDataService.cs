using RegistryDb.Models;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.ViewModel;
using RegistryServices.ViewModel.KumiAccounts;

namespace RegistryWeb.DataServices
{

    public class KumiPaymentsDataService : ListDataService<KumiPaymentsVM, KumiPaymentsFilter>
    {
        public KumiPaymentsDataService(RegistryContext registryContext, AddressesDataService addressesDataService) : base(registryContext, addressesDataService)
        {
        }
    }
}
