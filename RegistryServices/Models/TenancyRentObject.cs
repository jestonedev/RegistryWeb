using RegistryDb.Models.Entities.Tenancies;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class TenancyRentObject
    {
        public Address Address { get; set; }
        public double TotalArea { get; set; }
        public double LivingArea { get; set; }
        public double? RentArea { get; set; }
        public decimal Payment { get; set; }
        public decimal PaymentAfter28082019 { get; set; }
        public List<TenancyPaymentHistory> TenancyPaymentHistory { get; set; }
    }
}
