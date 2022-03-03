using RegistryDb.Models.Entities;

namespace RegistryServices.ViewModel.Payments
{
    public class PaymentHistoryVM
    {
        public TenancyPaymentHistory TenancyPaymentHistory { get; set; }
        public string ObjectDescription { get; set; }
    }
}
