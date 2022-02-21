using RegistryDb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class KumiAccountTenancyInfoVM
    {
        public TenancyProcess TenancyProcess { get; set; }
        public TenancyPerson Tenant { get; set; }
        public List<TenancyRentObject> RentObjects { get; set; }
    }
}
