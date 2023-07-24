using System;
using System.Collections.Generic;

namespace InvoiceGenerator
{
    public class InvoiceGeneratorParam
    {
        public int IdAccount { get; set; }
        public string Address { get; set; }
        public string Account { get; set; }
        public string Tenant { get; set; }
        public DateTime OnDate { get; set; }
        public string BalanceInput { get; set; }
        public string ChargingTenancy { get; set; }
        public string ChargingPenalty { get; set; }
        public string Payed { get; set; }
        public string RecalcTenancy { get; set; }
        public string RecalcPenalty { get; set; }
        public string BalanceOutput { get; set; }
        public string TotalArea { get; set; }
        public string TextMessage { get; set; }
        public int Prescribed { get; set; }
        public int OrderGroup { get; set; }
        public List<string> Emails { get; set; }

        public InvoiceGeneratorParam()
        {
            OrderGroup = -1;
        }
    }
}
