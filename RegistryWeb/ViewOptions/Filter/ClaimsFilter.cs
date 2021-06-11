using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewOptions.Filter
{
    public class ClaimsFilter : FilterAddressOptions
    {
        public int? IdClaim { get; set; }
        public int? IdAccount { get; set; }
        public string Account { get; set; }
        public int? IdClaimState { get; set; }
        public DateTime? ClaimStateDate { get; set; }
        public int? ClaimStateDateOp { get; set; }
        public string Crn { get; set; }
        public string RawAddress { get; set; }
        public string IdStreet { get; set; }
        public string IdRegion { get; set; }
        public string House { get; set; }
        public string PremisesNum { get; set; }
        public int? IdSubPremises { get; set; }
        public int? IdPremises { get; set; }
        public int? IdBuilding { get; set; }
        public DateTime? AtDate { get; set; }
        public string CourtOrderNum { get; set; }
        public int BalanceOutputTotalOp { get; set; }
        public decimal? BalanceOutputTotal { get; set; }
        public int BalanceOutputTenancyOp { get; set; }
        public decimal? BalanceOutputTenancy { get; set; }
        public int BalanceOutputPenaltiesOp { get; set; }
        public decimal? BalanceOutputPenalties { get; set; }
        public int BalanceOutputDgiPadunPkkOp { get; set; }
        public decimal? BalanceOutputDgiPadunPkk { get; set; }
        public int AmountTotalOp { get; set; }
        public decimal? AmountTotal { get; set; }
        public int AmountTenancyOp { get; set; }
        public decimal? AmountTenancy { get; set; }
        public int AmountPenaltiesOp { get; set; }
        public decimal? AmountPenalties { get; set; }
        public int AmountDgiPadunPkkOp { get; set; }
        public decimal? AmountDgiPadunPkk { get; set; }

        public ClaimsFilter()
        {
            AmountDgiPadunPkkOp = 2;
            AmountPenaltiesOp = 2;
            AmountTenancyOp = 2;
            AmountTotalOp = 2;
            BalanceOutputDgiPadunPkkOp = 2;
            BalanceOutputPenaltiesOp = 2;
            BalanceOutputTenancyOp = 2;
            BalanceOutputTotalOp = 2;
            ClaimStateDateOp = 2;
        }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty() && IdBuilding == null && IdPremises == null && IdSubPremises == null && IdAccount == null;
        }

        public bool IsModalEmpty()
        {
            return Account == null && Crn == null && IdClaim == null && RawAddress == null &&
                IdRegion == null && IdStreet == null && House == null && PremisesNum == null && IdClaimState == null &&
                ClaimStateDate == null && AtDate == null && CourtOrderNum == null &&
                 BalanceOutputTotal == null && BalanceOutputTenancy == null && BalanceOutputPenalties == null && BalanceOutputDgiPadunPkk == null &&
                 AmountTotal == null && AmountTenancy == null && AmountPenalties == null && AmountDgiPadunPkk == null;
        }
    }
}
