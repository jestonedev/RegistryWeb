using System;

namespace RegistryPaymentsLoader.Models
{
    public class KumiPaymentExtract
    {
        public string Guid { get; set; }
        public string CodeDoc { get; set; }
        public string NumDoc { get; set; }
        public DateTime DateDoc { get; set; }
        public decimal SumIn { get; set; }
        public decimal SumZach { get; set; }
        public string Kbk { get; set; }
        public string KbkType { get; set; }
        public string TargetCode { get; set; }
        public string Okato { get; set; }
        public string InnAdb { get; set; }
        public string KppAdb { get; set; }
    }
}