using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RegistryWeb.Models.SqlViews;

namespace RegistryWeb.Models.Entities
{
    public class KumiPayment
    {
        public KumiPayment()
        {
            PaymentCharges = new List<KumiPaymentCharge>();
        }

        public int IdPayment { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Payer { get; set; }
        public string Purpose { get; set; }
        public string Description { get; set; } 
        public byte IsManual { get; set; }
        public byte IsPosted { get; set; }
        public byte Deleted { get; set; }
        public virtual IList<KumiPaymentCharge> PaymentCharges { get; set; }
        public virtual IList<KumiPaymentClaim> PaymentClaims { get; set; }
    }
}
