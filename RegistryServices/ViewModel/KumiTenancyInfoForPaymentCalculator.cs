using RegistryDb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryWeb.ViewModel
{
    public class KumiTenancyInfoForPaymentCalculator
    {
        public int IdProcess { get; set; }
        public string RegistrationNum { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public TenancyPerson Tenant { get; set; }
        public List<RentPaymentForPaymentCalculator> RentPayments { get; set; }
        public List<RentPeriodForPaymentCalculator> RentPeriods { get; set; }
    }
}
