using RegistryDb.Models.SqlViews;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using RegistryWeb.ViewModel;
using RegistryDb.Models.Entities.Payments;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;

namespace RegistryServices.ViewModel.Payments
{
    public class PaymentsVM : ListVM<PaymentsFilter>
    {
        public IEnumerable<Payment> Payments { get; set; }
        public Dictionary<int, List<Address>> RentObjects { get; set; }
        public Dictionary<int, List<ClaimInfo>> ClaimsByAddresses { get; set; }
        public IEnumerable<KladrStreet> Streets { get; set; }
        public IEnumerable<KladrRegion> Regions { get; set; }
        public SelectList KladrRegionsList { get; set; }
        public SelectList KladrStreetsList { get; set; }
        public SelectList BuildingManagmentOrgsList { get; set; }
        public Dictionary<int, DateTime> MonthsList { get; set; }

    }
}
