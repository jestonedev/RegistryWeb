using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    [Serializable]
    public class KumiPaymentKind
    {
        public KumiPaymentKind()
        {
            Payments = new List<KumiPayment>();
        }

        public int IdPaymentKind { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public virtual IList<KumiPayment> Payments { get; set; }
    }
}