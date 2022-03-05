using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    public class KumiPaymentUf
    {
        public int IdPaymentUf { get; set; }
        public int IdPayment { get; set; }
        public string NumUf { get; set; }
        public DateTime DateUf { get; set; }
        public decimal? Sum { get; set; }
        public string Purpose { get; set; }
        public string Kbk { get; set; }
        public int? IdKbkType { get; set; }
        public string TargetCode { get; set; }
        public string Okato { get; set; }
        public string RecipientInn { get; set; }
        public string RecipientKpp { get; set; }
        public string RecipientName { get; set; }
        public string RecipientAccount { get; set; }
        public byte Deleted { get; set; }
        public virtual KumiPayment Payment { get; set; }
        public virtual KumiKbkType KbkType { get; set; }
    }
}
