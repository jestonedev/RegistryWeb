using Microsoft.AspNetCore.Mvc.Rendering;
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
        public Dictionary<int, List<KumiAccountTenancyInfoVM>> TenancyInfo { get; set; }
        public Dictionary<int, List<ClaimInfo>> ClaimsInfo { get; set; }
        public SelectList KladrRegionsList { get; set; }
        public SelectList KladrStreetsList { get; set; }
        public SelectList AccountStates { get; set; }
    }
}
