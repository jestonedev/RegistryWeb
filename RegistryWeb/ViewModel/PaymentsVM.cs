using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class PaymentsVM : ListVM<PaymentsFilter>
    {
        public IEnumerable<Payment> Payments { get; set; }
        public Dictionary<int, List<Address>> RentObjects { get; set; }
        public Dictionary<int, List<ClaimInfo>> ClaimsByAddresses { get; set; }
        public IEnumerable<KladrStreet> Streets { get; set; }
    }
}
