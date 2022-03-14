using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiPaymentsVM : ListVM<KumiPaymentsFilter>
    {
        public IEnumerable<KumiPayment> Payments { get; set; }
        public SelectList PaymentSourcesList { get; set; }
    }
}
