using RegistryDb.Models.Entities.KumiAccounts;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiChargeCorrectionsVM : ListVM<FilterOptions>
    {
        public IEnumerable<KumiChargeCorrection> ChargeCorrections { get; set; }
        public KumiAccount Account { get; set; }
    }
}
