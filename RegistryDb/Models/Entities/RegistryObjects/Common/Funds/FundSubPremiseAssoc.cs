using RegistryDb.Models.Entities.RegistryObjects.Premises;

namespace RegistryDb.Models.Entities.RegistryObjects.Common.Funds
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
