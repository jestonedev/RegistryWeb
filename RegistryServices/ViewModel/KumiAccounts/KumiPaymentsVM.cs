using RegistryDb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiPaymentsVM : ListVM<KumiPaymentsFilter>
    {
        public IEnumerable<KumiPayment> Payments { get; set; }
    }
}
