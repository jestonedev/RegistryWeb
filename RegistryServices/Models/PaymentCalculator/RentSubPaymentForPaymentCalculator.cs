using RegistryDb.Models.Entities;
using RegistryWeb.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryWeb.ViewModel
{
    public class RentSubPaymentForPaymentCalculator
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal PaymentMonth { get; set; }
    }
}
