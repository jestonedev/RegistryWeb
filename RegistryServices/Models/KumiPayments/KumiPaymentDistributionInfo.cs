using System;

namespace RegistryServices.Models.KumiPayments
{
    public class KumiPaymentDistributionInfo
    {
        public int IdPayment { get; set; }
        public decimal Sum { get; set; }
        public decimal DistrubutedToPenaltySum { get; set; }
        public decimal DistrubutedToTenancySum { get; set; }
    }
}
