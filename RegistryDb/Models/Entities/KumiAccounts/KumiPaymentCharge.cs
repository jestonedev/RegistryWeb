using System;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    [Serializable]
    public class KumiPaymentCharge
    {
        public KumiPaymentCharge()
        {
        }

        public int IdAssoc { get; set; }
        public int IdPayment { get; set; }
        public int IdCharge { get; set; }
        public DateTime Date { get; set; }
        
        // Найм
        public decimal TenancyValue { get; set; }
        public decimal PenaltyValue { get; set; }
        
        // ДГИ
        public decimal DgiValue { get; set; }

        // ПКК
        public decimal PkkValue { get; set; }

        // Падун
        public decimal PadunValue { get; set; }

        public virtual KumiPayment Payment { get; set; }
        public virtual KumiCharge Charge { get; set; }
        public int? IdDisplayCharge { get; set; }
        public virtual KumiCharge DisplayCharge { get; set; }
    }
}
