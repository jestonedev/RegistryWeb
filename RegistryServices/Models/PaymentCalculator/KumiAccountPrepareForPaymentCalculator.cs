using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.Tenancies;
using RegistryServices.ViewModel.KumiAccounts;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class KumiAccountPrepareForPaymentCalculator
    {
        public KumiAccount Account { get; set; } // with charges
        public List<KumiAccountTenancyInfoVM> TenancyInfo { get; set; }
        public Dictionary<int, List<TenancyPaymentHistory>> TenancyPaymentHistories { get; set; }
        public List<KumiPayment> Payments { get; set; } // payments by account
        public List<Claim> Claims { get; set; }
    }
}
