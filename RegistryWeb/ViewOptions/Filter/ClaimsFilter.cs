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
        public bool ClaimStateLastOnly { get; set; }
        public string Crn { get; set; }
        public string RawAddress { get; set; }
        public string IdStreet { get; set; }
        public string House { get; set; }
        public string PremisesNum { get; set; }
        public int? IdSubPremises { get; set; }
        public int? IdPremises { get; set; }
        public int? IdBuilding { get; set; }
        public DateTime? AtDate { get; set; }
        public string CourtOrderNum { get; set; }
        public decimal? BalanceOutputTenancy { get; set; }
        public decimal? BalanceOutputPenalties { get; set; }
        public decimal? BalanceOutputDgiPadunPkk { get; set; }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty() && IdBuilding == null && IdPremises == null && IdSubPremises == null && IdAccount == null;
        }

        public bool IsModalEmpty()
        {
            return Account == null && Crn == null && IdClaim == null && RawAddress == null &&
                IdStreet == null && House == null && PremisesNum == null && IdClaimState == null &&
                ClaimStateDate == null && AtDate == null && CourtOrderNum == null &&
                BalanceOutputTenancy == null && BalanceOutputPenalties == null && BalanceOutputDgiPadunPkk == null;
        }
    }
}
