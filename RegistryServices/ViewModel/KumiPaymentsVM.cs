using RegistryDb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class KumiPaymentsVM : ListVM<KumiPaymentsFilter>
    {
        public IEnumerable<KumiPayment> Payments { get; set; }
    }
}
