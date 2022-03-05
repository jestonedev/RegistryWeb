using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.Payments;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryDb.Models.SqlViews;
using RegistryServices.ViewModel.KumiAccounts;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.Claims
{
    public class ClaimsVM : ListVM<ClaimsFilter>
    {
        public IEnumerable<Claim> Claims { get; set; }
        public Dictionary<int, List<Address>> RentObjectsBks { get; set; }
        public Dictionary<int, List<KumiAccountTenancyInfoVM>> TenancyInfoKumi { get; set; }
        public Dictionary<int, Payment> LastPaymentInfo { get; set; }
        public IEnumerable<KladrStreet> Streets { get; set; }
        public IEnumerable<ClaimStateType> StateTypes { get; set; }
        public IEnumerable<KladrRegion> Regions { get; set; }
        public SelectList KladrRegionsList { get; set; }
        public SelectList KladrStreetsList { get; set; }
    }
}
