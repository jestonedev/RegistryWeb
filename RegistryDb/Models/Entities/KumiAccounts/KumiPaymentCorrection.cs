using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    [Serializable]
    public class KumiPaymentCorrection
    {
        public int IdCorrection { get; set; }
        public int IdPayment { get; set; }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public DateTime Date { get; set; }
        public virtual KumiPayment Payment { get; set; }
    }
}
