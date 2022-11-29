using RegistryDb.Models.Entities.RegistryObjects.Premises;

namespace RegistryDb.Models.Entities.RegistryObjects.Common.Restrictions
{
    public partial class RestrictionPremiseAssoc
    {
        public int IdPremises { get; set; }
        public int IdRestriction { get; set; }
        public byte Deleted { get; set; }

        public virtual Restriction RestrictionNavigation { get; set; }
        public virtual Premise PremisesNavigation { get; set; }
    }
}
