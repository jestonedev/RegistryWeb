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
        
        // Найм
        public decimal TenancyValue { get; set; }
        public decimal PenaltyValue { get; set; }
        public decimal PaymentTenancyValue { get; set; }
        public decimal PaymentPenaltyValue { get; set; }
        
        // ДГИ
        public decimal DgiValue { get; set; }
        public decimal PaymentDgiValue { get; set; }

        // ПКК
        public decimal PkkValue { get; set; }
        public decimal PaymentPkkValue { get; set; }
        
        // Падун
        public decimal PadunValue { get; set; }
        public decimal PaymentPadunValue { get; set; }

        public string User { get; set; }

        public virtual KumiAccount Account { get; set; }
    }
}
