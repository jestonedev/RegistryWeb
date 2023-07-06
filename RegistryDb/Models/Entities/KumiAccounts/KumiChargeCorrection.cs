using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    [Serializable]
    public class KumiChargeCorrection
    {
        public int IdCorrection { get; set; }
        public int IdAccount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal TenancyValue { get; set; }
        public decimal PenaltyValue { get; set; }
        public decimal PaymentTenancyValue { get; set; }
        public decimal PaymentPenaltyValue { get; set; }
        public virtual KumiAccount Account { get; set; }
    }
}
