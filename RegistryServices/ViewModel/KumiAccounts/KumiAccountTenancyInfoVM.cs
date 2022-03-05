using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;
using RegistryWeb.ViewModel;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiAccountTenancyInfoVM
    {
        public TenancyProcess TenancyProcess { get; set; }
        public TenancyPerson Tenant { get; set; }
        public List<TenancyRentObject> RentObjects { get; set; }
    }
}
