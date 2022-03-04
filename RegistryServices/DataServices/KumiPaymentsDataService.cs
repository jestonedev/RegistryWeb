using RegistryDb.Models;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.ViewModel;
using RegistryServices.ViewModel.KumiAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RegistryWeb.DataServices
{

    public class KumiPaymentsDataService : ListDataService<KumiPaymentsVM, KumiPaymentsFilter>
    {
        public KumiPaymentsDataService(RegistryContext registryContext, AddressesDataService addressesDataService) : base(registryContext, addressesDataService)
        {
        }
    }
}
