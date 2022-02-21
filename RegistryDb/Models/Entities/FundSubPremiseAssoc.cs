namespace RegistryDb.Models.Entities
{
    public partial class FundSubPremiseAssoc
    {
        public int IdSubPremises { get; set; }
        public int IdFund { get; set; }
        public byte Deleted { get; set; }

        public virtual FundHistory IdFundNavigation { get; set; }
        public virtual SubPremise IdSubPremisesNavigation { get; set; }
    }
}
