using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    [Serializable]
    public class KumiPaymentGroup
    {
        public KumiPaymentGroup()
        {
            Payments = new List<KumiPayment>();
            PaymentGroupFiles = new List<KumiPaymentGroupFile>();
        }

        public int IdGroup { get; set; }
        public DateTime Date { get; set; }
        public string User { get; set; }

        public virtual IList<KumiPayment> Payments { get; set; }
        public virtual IList<KumiMemorialOrder> MemorialOrders { get; set; }
        public virtual IList<KumiPaymentGroupFile> PaymentGroupFiles { get; set; }
    }
}