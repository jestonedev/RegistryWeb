using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    public class TenancyRentObject
    {
        public int IdProcess { get; set; }
        public Address Address { get; set; }
        public double TotalArea { get; set; }
        public double LivingArea { get; set; }
        public double? RentArea { get; set; }
        public decimal Payment { get; set; }
        public decimal PaymentAfter28082019 { get; set; }
    }
}
