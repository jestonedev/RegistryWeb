using RegistryDb.Models.Entities;
using RegistryWeb.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryWeb.ViewModel
{
    public class RentPaymentForPaymentCalculator
    {
        public int IdObject { get; set; }
        public AddressTypes AddressType { get; set; }
        public DateTime FromDate { get; set; }
        public decimal Payment { get; set; }
    }
}
