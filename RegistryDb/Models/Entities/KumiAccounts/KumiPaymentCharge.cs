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
        public virtual KumiPayment Payment { get; set; }
        public virtual KumiCharge Charge { get; set; }
    }
}
