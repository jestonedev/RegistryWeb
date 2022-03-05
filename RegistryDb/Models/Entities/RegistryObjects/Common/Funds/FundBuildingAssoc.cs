using RegistryDb.Models.Entities.RegistryObjects.Buildings;

namespace RegistryDb.Models.Entities.RegistryObjects.Common.Funds
{
    public partial class FundBuildingAssoc
    {
        public int IdBuilding { get; set; }
        public int IdFund { get; set; }
        public byte Deleted { get; set; }

        public virtual Building IdBuildingNavigation { get; set; }
        public virtual FundHistory IdFundNavigation { get; set; }
    }
}
