using RegistryDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using RegistryDb.Models.Entities;
using RegistryDb.Models.SqlViews;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.ViewModel;

namespace RegistryWeb.DataServices
{

    public class KumiPaymentsDataService : ListDataService<KumiPaymentsVM, KumiPaymentsFilter>
    {
        public KumiPaymentsDataService(RegistryContext registryContext, AddressesDataService addressesDataService) : base(registryContext, addressesDataService)
        {
        }
    }
}
