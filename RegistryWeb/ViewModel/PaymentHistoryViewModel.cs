using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class PaymentHistoryViewModel
    {
        public TenancyPaymentHistory TenancyPaymentHistory { get; set; }
        public string ObjectDescription { get; set; }
    }
}
