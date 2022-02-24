using RegistryDb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryWeb.ViewModel
{
    public class KumiAccountPrepareForPaymentCalculator
    {
        public KumiAccount Account { get; set; } // with charges
        public List<KumiAccountTenancyInfoVM> TenancyInfo { get; set; }
        public Dictionary<int, List<TenancyPaymentHistory>> TenancyPaymentHistories { get; set; }
        public List<KumiPayment> Payments { get; set; } // payments by account
    }
}
