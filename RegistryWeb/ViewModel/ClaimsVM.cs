using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class ClaimsVM : ListVM<ClaimsFilter>
    {
        public IEnumerable<Claim> Claims { get; set; }
        public Dictionary<int, List<Address>> RentObjects { get; set; }
        public Dictionary<int, Payment> LastPaymentInfo { get; set; }
        public IEnumerable<KladrStreet> Streets { get; set; }
        public IEnumerable<ClaimStateType> StateTypes { get; set; }
    }
}
