using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class InvoiceGeneratorParam
    {
        public int IdAcconut { get; set; }
        public string Address { get; set; }
        public string Account { get; set; }
        public string Tenant { get; set; }
        public DateTime OnData { get; set; }
        public string BalanceInput { get; set; }
        public string Charging { get; set; }
        public string Payed { get; set; }
        public string Recalc { get; set; }
        public string BalanceOutput { get; set; }
        public string TotalArea { get; set; }
        public string TextMessage { get; set; }
        public int Prescribed { get; set; }
        public List<string> Emails { get; set; }
    }
}
