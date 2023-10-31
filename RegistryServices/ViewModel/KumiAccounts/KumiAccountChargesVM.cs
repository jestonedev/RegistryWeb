using RegistryDb.Models.Entities.KumiAccounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiAccountChargesVM
    {
        public IEnumerable<KumiCharge> Charges { get; set; }
        public IEnumerable<KumiChargeCorrection> Corrections { get; set; }
    }
}
