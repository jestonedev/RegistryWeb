using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public class KumiKbkType
    {
        public KumiKbkType()
        {
            Payments = new List<KumiPayment>();
        }

        public int IdKbkType { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public virtual IList<KumiPayment> Payments { get; set; }
        public virtual IList<KumiPaymentUf> Paymentufs { get; set; }
        public virtual IList<KumiMemorialOrder> MemorialOrders { get; set; }
    }
}