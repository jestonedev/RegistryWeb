using RegistryDb.Models;
using RegistryDb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class KumiAccountTenancyInfoVM
    {
        public TenancyProcess TenancyProcess { get; set; }
        public TenancyPerson Tenant { get; set; }
        public List<TenancyRentObject> RentObjects { get; set; }
    }
}
