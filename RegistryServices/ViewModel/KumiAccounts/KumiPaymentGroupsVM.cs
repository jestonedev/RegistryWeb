using RegistryDb.Models.Entities.KumiAccounts;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiPaymentGroupsVM: ListVM<KumiPaymentsFilter>
    {
        public IEnumerable<KumiPaymentGroup> paymentGroups { get; set; }
    }
}
