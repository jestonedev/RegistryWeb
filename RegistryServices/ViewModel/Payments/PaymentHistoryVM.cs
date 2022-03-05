using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryServices.ViewModel.Payments
{
    public class PaymentHistoryVM
    {
        public TenancyPaymentHistory TenancyPaymentHistory { get; set; }
        public string ObjectDescription { get; set; }
    }
}
