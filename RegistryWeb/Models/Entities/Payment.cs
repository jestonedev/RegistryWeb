using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RegistryWeb.Models.SqlViews;

namespace RegistryWeb.Models.Entities
{
    public class Payment
    {
        public Payment()
        {
        }

        public int IdPayment { get; set; }
        public int IdAccount { get; set; }
        public DateTime Date { get; set; }
        public string Tenant { get; set; }
        public double TotalArea { get; set; }
        public double LivingArea { get; set; }
        public int Prescribed { get; set; }
        public decimal BalanceInput { get; set; }
        public decimal BalanceTenancy { get; set; }
        public decimal BalanceDgi { get; set; }
        public decimal? BalancePadun { get; set; }
        public decimal? BalancePkk { get; set; }
        public decimal? BalanceInputPenalties { get; set; }
        public decimal ChargingTenancy { get; set; }
        public decimal ChargingTotal { get; set; }
        public decimal ChargingDgi { get; set; }
        public decimal? ChargingPadun { get; set; }
        public decimal? ChargingPkk { get; set; }
        public decimal? ChargingPenalties { get; set; }
        public decimal RecalcTenancy { get; set; }
        public decimal RecalcDgi { get; set; }
        public decimal? RecalcPadun { get; set; }
        public decimal? RecalcPkk { get; set; }
        public decimal? RecalcPenalties { get; set; }
        public decimal PaymentTenancy { get; set; }
        public decimal PaymentDgi { get; set; }
        public decimal? PaymentPadun { get; set; }
        public decimal? PaymentPkk { get; set; }
        public decimal? PaymentPenalties { get; set; }
        public decimal TransferBalance { get; set; }
        public decimal BalanceOutputTotal { get; set; }
        public decimal BalanceOutputTenancy { get; set; }
        public decimal BalanceOutputDgi { get; set; }
        public decimal? BalanceOutputPadun { get; set; }
        public decimal? BalanceOutputPkk { get; set; }
        public decimal? BalanceOutputPenalties { get; set; }
        public virtual PaymentAccount PaymentAccountNavigation { get; set; }
    }
}
