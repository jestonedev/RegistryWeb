using System;
using System.Collections.Generic;
using System.Text;

namespace InvoiceGenerator
{
    public class InvoiceGeneratorParamTyped
    {
        public int IdAccount { get; set; }
        public string Address { get; set; }
        public string PostIndex { get; set; }
        public string Account { get; set; }
        public string AccountGisZkh { get; set; }
        public string Tenant { get; set; }
        public DateTime OnDate { get; set; }
        public decimal BalanceInput { get; set; }
        public decimal Charging { get { return ChargingTenancy + ChargingPenalty; } }
        public decimal Payed { get; set; }
        public decimal BalanceOutput { get; set; }
        public decimal Tariff { get; set; }
        public float TotalArea { get; set; }
        public int Prescribed { get; set; }
        public string Email { get; set; }
        public string MoveToFileName { get; set; }
        public string MessageBody { get; set; }
        public decimal ChargingTenancy { get; set; }
        public decimal ChargingPenalty { get; set; }
        public decimal RecalcTenancy { get; set; }
        public decimal RecalcPenalty { get; set; }
    }
}
