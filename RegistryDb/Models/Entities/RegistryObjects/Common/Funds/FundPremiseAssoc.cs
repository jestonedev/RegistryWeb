using RegistryDb.Models.Entities.RegistryObjects.Premises;

namespace RegistryDb.Models.Entities.RegistryObjects.Common.Funds
{
    public partial class FundPremiseAssoc
    {
        public int IdPremises { get; set; }
        public int IdFund { get; set; }
        public byte Deleted { get; set; }

        public virtual FundHistory IdFundNavigation { get; set; }
        public virtual Premise IdPremisesNavigation { get; set; }
    }
}
