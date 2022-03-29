using System;
using RegistryWeb.Models;

namespace RegistryWeb.ViewOptions.Filter
{
    public class ClaimsFilter : FilterAddressOptions
    {
        public int? IdClaim { get; set; }
        public int? IdAccount { get; set; }
        public string Account { get; set; }
        public int? IdClaimState { get; set; }
        public bool IsCurrentState { get; set; }
        public DateTime? ClaimStateDateFrom { get; set; }
        public DateTime? ClaimStateDateTo { get; set; }        
        public ComparisonSignEnum ClaimStateDateOp { get; set; }
        public DateTime? ClaimFormStatementSSPDateFrom { get; set; }
        public DateTime? ClaimFormStatementSSPDateTo { get; set; }
        public ComparisonSignEnum ClaimFormStatementSSPDateOp { get; set; }
        public DateTime? ClaimDirectionDateFrom { get; set; }
        public DateTime? ClaimDirectionDateTo { get; set; }
        public ComparisonSignEnum ClaimDirectionDateOp { get; set; }
        public DateTime? CourtOrderDateFrom { get; set; }
        public DateTime? CourtOrderDateTo { get; set; }
        public ComparisonSignEnum CourtOrderDateOp { get; set; }
        public DateTime? ObtainingCourtOrderDateFrom { get; set; }
        public DateTime? ObtainingCourtOrderDateTo { get; set; }
        public ComparisonSignEnum ObtainingCourtOrderDateOp { get; set; }
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
        }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty() && IdBuilding == null && IdPremises == null && IdSubPremises == null && IdAccount == null;
        }

        public bool IsModalEmpty()
        {
            return Account == null && Crn == null && IdClaim == null && RawAddress == null &&
                IdRegion == null && IdStreet == null && House == null && PremisesNum == null &&
                AtDate == null && CourtOrderNum == null &&
                BalanceOutputTotal == null && BalanceOutputTenancy == null && BalanceOutputPenalties == null && BalanceOutputDgiPadunPkk == null &&
                AmountTotal == null && AmountTenancy == null && AmountPenalties == null && AmountDgiPadunPkk == null &&
                IdClaimState == null && IsCurrentState == false &&
                ClaimStateDateFrom == null && ClaimStateDateTo == null &&
                ClaimFormStatementSSPDateFrom == null && ClaimFormStatementSSPDateTo == null &&
                ClaimDirectionDateFrom == null && ClaimDirectionDateTo == null &&
                CourtOrderDateFrom == null && CourtOrderDateTo == null &&
                ObtainingCourtOrderDateFrom == null && ObtainingCourtOrderDateTo == null;
        }
    }
}
