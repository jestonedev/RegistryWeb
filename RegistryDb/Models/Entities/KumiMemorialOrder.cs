using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public class KumiMemorialOrder
    {
        public int IdOrder { get; set; }
        public int IdPayment { get; set; }
        public string Guid { get; set; }
        public string NumDocument { get; set; }
        public DateTime DateDocument { get; set; }
        public decimal SumIn { get; set; }
        public decimal SumZach { get; set; }
        public string Kbk { get; set; }
        public int IdKbkType { get; set; }
        public string TargetCode { get; set; }
        public string Okato { get; set; }
        public string InnAdb { get; set; }
        public string KppAdb { get; set; }
        public byte Deleted { get; set; }
        public virtual KumiPayment Payment { get; set; }
        public virtual KumiKbkType KbkType { get; set; }
    }
}
