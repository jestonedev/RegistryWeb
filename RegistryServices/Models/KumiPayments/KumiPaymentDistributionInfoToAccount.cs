using RegistryServices.Enums;
using System;

namespace RegistryServices.Models.KumiPayments
{
    public class KumiPaymentDistributionInfoToAccount: KumiPaymentDistributionInfoToObject
    {
        public int IdAccount { get; set; }
        public string Account { get; set; }
    }
}
