using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    [Serializable]
    public class KumiMemorialOrderPaymentAssoc
    {
        public int IdAssoc { get; set; }
        public int IdOrder { get; set; }
        public int IdPayment { get; set; }
        public virtual KumiPayment Payment { get; set; }
        public virtual KumiMemorialOrder Order { get; set; }
    }
}
