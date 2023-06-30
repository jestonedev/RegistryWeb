using System;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    [Serializable]
    public class KumiPaymentGroupLog
    {
        [Key]
        public int IdLog { get; set; }
        public int IdGroup { get; set; }
        public string Log { get; set; }
        public virtual KumiPaymentGroup PaymentGroup { get; set; }
    }
}