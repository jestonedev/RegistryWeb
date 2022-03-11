using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    [Serializable]
    public class KumiPayerStatus
    {
        public KumiPayerStatus()
        {
            Payments = new List<KumiPayment>();
        }

        public int IdPayerStatus { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public virtual IList<KumiPayment> Payments { get; set; }
    }
}