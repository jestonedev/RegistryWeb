using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryServices.Models.KumiPayments;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiChargeCorrectionsVM : ListVM<FilterOptions>
    {
        public IEnumerable<KumiChargeCorrection> ChargeCorrections { get; set; }
        public KumiAccount Account { get; set; }
    }
}
