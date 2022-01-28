using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RegistryWeb.Models.SqlViews;

namespace RegistryWeb.Models.Entities
{
    public class KumiCharge
    {
        public KumiCharge()
        {
            PaymentCharges = new List<KumiPaymentCharge>();
        }

        public int IdCharge { get; set; }
        public int IdAccount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal InputTenancy { get; set; }
        public decimal InputPenalty { get; set; }
        public decimal ChargeTenancy { get; set; }
        public decimal ChargePenalty { get; set; }
        public decimal PaymentTenancy { get; set; }
        public decimal PaymentPenalty { get; set; }
        public decimal RecalcTenancy { get; set; }
        public decimal RecalcPenalty { get; set; }
        public decimal OutputTenancy { get; set; }
        public decimal OutputPenalty { get; set; }
        public byte Deleted { get; set; }
        public virtual KumiAccount Account { get; set; }
        public virtual IList<KumiPaymentCharge> PaymentCharges { get; set; }
    }
}
