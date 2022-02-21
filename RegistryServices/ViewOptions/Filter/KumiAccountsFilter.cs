namespace RegistryWeb.ViewOptions.Filter
{
    public class KumiAccountsFilter : FilterAddressOptions
    {
        public string FrontSideAccount { get; set; }
        public string Account { get; set; }
        public string AccountGisZkh { get; set; }
        public int? IdAccountState { get; set; }
        public string Tenant { get; set; }
        public bool Emails { get; set; }
        public string IdStreet { get; set; }
        public string House { get; set; }
        public string PremisesNum { get; set; }
        public int? IdSubPremises { get; set; }
        public int? IdPremises { get; set; }
        public int? IdBuilding { get; set; }
        public int? IdPreset { get; set; } // Поисковые пресеты
        public int? IdClaimsBehavior { get; set; } // Поведение при фильтрации с учетом исковых работ
        public string IdRegion { get; set; }
        public int CurrentBalanceTenancyOp { get; set; }
        public decimal? CurrentBalanceTenancy { get; set; }
        public int CurrentBalancePenaltyOp { get; set; }
        public decimal? CurrentBalancePenalty { get; set; }
        public int CurrentBalanceTotalOp { get; set; }
        public decimal? CurrentBalanceTotal { get; set; }

        public KumiAccountsFilter()
        {
            CurrentBalanceTenancyOp = 2;
            CurrentBalancePenaltyOp = 2;
            CurrentBalanceTotalOp = 2;
        }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty() && IdBuilding == null && IdPremises == null && IdSubPremises == null && FrontSideAccount == null;
        }

        public bool IsModalEmpty()
        {
            return Account == null && AccountGisZkh == null && Tenant == null &&
                IdRegion == null && IdStreet == null && House == null && PremisesNum == null && IdPreset == null && IdClaimsBehavior == null &&
                CurrentBalanceTenancy == null && CurrentBalancePenalty == null && CurrentBalanceTotal == null && !Emails && IdAccountState == null;
        }
    }
}
