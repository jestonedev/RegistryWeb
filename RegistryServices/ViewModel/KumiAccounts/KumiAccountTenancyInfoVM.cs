using RegistryDb.Models.Entities.Tenancies;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryWeb.ViewModel;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiAccountTenancyInfoVM
    {
        public TenancyProcess TenancyProcess { get; set; }
        public TenancyPerson Tenant { get; set; }
        public KumiAccountsTenancyProcessesAssoc AccountAssoc { get; set; }
        public List<TenancyRentObject> RentObjects { get; set; }
    }
}
