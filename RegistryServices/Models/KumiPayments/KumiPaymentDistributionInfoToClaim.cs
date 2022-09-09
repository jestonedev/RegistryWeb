using RegistryServices.Enums;
using System;

namespace RegistryServices.Models.KumiPayments
{
    public class KumiPaymentDistributionInfoToClaim: KumiPaymentDistributionInfoToObject
    {
        public int IdClaim { get; set; }
        public int? IdAccountKumi { get; set; }
    }
}
