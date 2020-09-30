﻿using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewOptions.Filter
{
    public class PaymentsFilter : FilterAddressOptions
    {
        public string Crn { get; set; }
        public string Account { get; set; }
        public string Tenant { get; set; }
        public string RawAddress { get; set; }
        public string IdStreet { get; set; }
        public string House { get; set; }
        public string PremisesNum { get; set; }
        public int? IdSubPremises { get; set; }
        public int? IdPremises { get; set; }
        public int? IdBuilding { get; set; }
        public int? IdPreset { get; set; } // Поисковые пресеты
        public int? IdClaimsBehavior { get; set; } // Поведение при фильтрации с учетом исковых работ
        public DateTime? AtDate { get; set; }
        public int BalanceInputTotalOp { get; set; }
        public decimal? BalanceInputTotal { get; set;}
        public int BalanceInputTenancyOp { get; set; }
        public decimal? BalanceInputTenancy { get; set; }
        public int BalanceInputPenaltiesOp { get; set; }
        public decimal? BalanceInputPenalties { get; set; }
        public int BalanceInputDgiPadunPkkOp { get; set; }
        public decimal? BalanceInputDgiPadunPkk { get; set; }
        public int ChargingTotalOp { get; set; }
        public decimal? ChargingTotal { get; set; }
        public int ChargingTenancyOp { get; set; }
        public decimal? ChargingTenancy { get; set; }
        public int ChargingPenaltiesOp { get; set; }
        public decimal? ChargingPenalties { get; set; }
        public int ChargingDgiPadunPkkOp { get; set; }
        public decimal? ChargingDgiPadunPkk { get; set; }
        public int RecalcTenancyOp { get; set; }
        public decimal? RecalcTenancy { get; set; }
        public int RecalcPenaltiesOp { get; set; }
        public decimal? RecalcPenalties { get; set; }
        public int RecalcDgiPadunPkkOp { get; set; }
        public decimal? RecalcDgiPadunPkk { get; set; }
        public int TransferBalanceOp { get; set; }
        public decimal? TransferBalance { get; set; }
        public int PaymentTenancyOp { get; set; }
        public decimal? PaymentTenancy { get; set; }
        public int PaymentPenaltiesOp { get; set; }
        public decimal? PaymentPenalties { get; set; }
        public int PaymentDgiPadunPkkOp { get; set; }
        public decimal? PaymentDgiPadunPkk { get; set; }
        public int BalanceOutputTotalOp { get; set; }
        public decimal? BalanceOutputTotal { get; set; }
        public int BalanceOutputTenancyOp { get; set; }
        public decimal? BalanceOutputTenancy { get; set; }
        public int BalanceOutputPenaltiesOp { get; set; }
        public decimal? BalanceOutputPenalties { get; set; }
        public int BalanceOutputDgiPadunPkkOp { get; set; }
        public decimal? BalanceOutputDgiPadunPkk { get; set; }

        public PaymentsFilter()
        {
            BalanceInputDgiPadunPkkOp = 2;
            BalanceInputPenaltiesOp = 2;
            BalanceInputTenancyOp = 2;
            BalanceInputTotalOp = 2;
            BalanceOutputDgiPadunPkkOp = 2;
            BalanceOutputPenaltiesOp = 2;
            BalanceOutputTenancyOp = 2;
            BalanceOutputTotalOp = 2;
            ChargingDgiPadunPkkOp = 2;
            ChargingPenaltiesOp = 2;
            ChargingTenancyOp = 2;
            ChargingTotalOp = 2;
            PaymentDgiPadunPkkOp = 2;
            PaymentPenaltiesOp = 2;
            PaymentTenancyOp = 2;
            RecalcDgiPadunPkkOp = 2;
            RecalcPenaltiesOp = 2;
            RecalcTenancyOp = 2;
            TransferBalanceOp = 2;
        }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty() && IdBuilding == null && IdPremises == null && IdSubPremises == null;
        }

        public bool IsModalEmpty()
        {
            return Account == null && Crn == null && Tenant == null && RawAddress == null &&
                IdStreet == null && House == null && PremisesNum == null && IdPreset == null && IdClaimsBehavior == null &&
                BalanceInputTotal == null && BalanceInputTenancy == null && BalanceInputPenalties == null &&
                BalanceInputDgiPadunPkk == null && ChargingTotal == null && ChargingTenancy == null && ChargingPenalties == null &&
                ChargingDgiPadunPkk == null && RecalcTenancy == null && RecalcPenalties == null && RecalcDgiPadunPkk == null &&
                TransferBalance == null && PaymentTenancy == null && PaymentPenalties == null && PaymentDgiPadunPkk == null &&
                BalanceOutputTotal == null && BalanceOutputTenancy == null && BalanceOutputPenalties == null && BalanceOutputDgiPadunPkk == null;
        }
    }
}
