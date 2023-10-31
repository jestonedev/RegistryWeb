using System;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiActPaymentEventVM: IChargePaymentEventVM
    {
        public DateTime Date { get; set; }
        public decimal Tenancy { get; set; }
        public decimal TenancyTail { get; set; }
        public decimal Penalty { get; set; }
        public decimal PenaltyTail { get; set; }
        public int IdPayment { get; set; }
        public string NumDocument { get; set; }
        public DateTime? DateDocument { get; set; }
    }
}
