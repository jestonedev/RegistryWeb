using RegistryDb.Models.Entities.KumiAccounts;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiAccountChargesVM
    {
        public IEnumerable<KumiCharge> Charges { get; set; }
        public IEnumerable<KumiChargeCorrection> Corrections { get; set; }
    }
}
