using System;

namespace RegistryWeb.ViewModel
{
    public class RentSubPaymentForPaymentCalculator
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal PaymentMonth { get; set; }
    }
}
