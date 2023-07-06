using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.KumiAccounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryWeb.ViewModel
{
    public class KumiAccountInfoForPaymentCalculator
    {
        public int IdAccount { get; set; }
        public int IdState { get; set; }
        public string Account { get; set; }
        public DateTime? LastChargeDate { get; set; }
        public decimal CurrentBalanceTenancy { get; set; }
        public decimal CurrentBalancePenalty { get; set; }
        public List<KumiTenancyInfoForPaymentCalculator> TenancyInfo { get; set; }
        public List<KumiCharge> Charges { get; set; }
        public List<KumiPayment> Payments { get; set; }
        public List<Claim> Claims { get; set; }
        public List<KumiChargeCorrection> Corrections { get; set; }
    }
}
