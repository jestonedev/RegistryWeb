using RegistryDb.Models.Entities.RegistryObjects.Premises;

namespace RegistryDb.Models.Entities.RegistryObjects.Buildings.Litigations
{
    public partial class LitigationPremiseAssoc
    {
        public int IdPremises { get; set; }
        public int IdLitigation { get; set; }
        public byte Deleted { get; set; }

        public virtual Litigation LitigationNavigation { get; set; }
        public virtual Premise PremiseNavigation { get; set; }
    }
}
