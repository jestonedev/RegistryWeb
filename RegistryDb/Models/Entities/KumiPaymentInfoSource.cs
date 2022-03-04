using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public class KumiPaymentInfoSource
    {
        public KumiPaymentInfoSource()
        {
            Payments = new List<KumiPayment>();
        }

        public int IdSource { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public virtual IList<KumiPayment> Payments { get; set; }
    }
}