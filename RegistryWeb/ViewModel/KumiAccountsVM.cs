using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.Models.SqlViews;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class KumiAccountsVM : ListVM<KumiAccountsFilter>
    {
        public IEnumerable<KumiAccount> Accounts { get; set; }
        public Dictionary<int, List<TenancyRentObject>> RentObjects { get; set; }
    }
}
