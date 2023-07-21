using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryPaymentsLoader.Models
{
    public class ZfPayment
    {
        public string PayerName { get; set; }
        public string PayerInn { get; set; }
        public string PayerKpp { get; set; }
        public string NumDoc { get; set; }
        public DateTime? DateDoc { get; set; }
        public string RecipientInn { get; set; }
        public string RecipientKpp { get; set; }
        public string Kbk { get; set; }
        public string KbkType { get; set; }
        public string TargetCode { get; set; }
        public string Okato { get; set; }
        public decimal Sum { get; set; }
        public string Purpose { get; set; }
    }
}
