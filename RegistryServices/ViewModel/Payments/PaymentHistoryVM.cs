using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.Payments
{
    public class PaymentHistoryVM
    {
        public TenancyPaymentHistory TenancyPaymentHistory { get; set; }
        public List<TenancyProcess> TenancyProcesses { get; set; }
        public string ObjectDescription { get; set; }
    }
}
