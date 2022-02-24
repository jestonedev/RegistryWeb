using RegistryDb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryWeb.ViewModel
{
    public class KumiAccountInfoForPaymentCalculator
    {
        public int IdAccount { get; set; }
        public List<KumiTenancyInfoForPaymentCalculator> TenancyInfo { get; set; }
        public List<KumiCharge> Charges { get; set; }
        public List<KumiPayment> Payments { get; set; }
    }
}
