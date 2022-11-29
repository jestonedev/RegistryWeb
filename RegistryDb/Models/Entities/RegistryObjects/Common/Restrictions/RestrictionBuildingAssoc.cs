using RegistryDb.Models.Entities.RegistryObjects.Buildings;

namespace RegistryDb.Models.Entities.RegistryObjects.Common.Restrictions
{
    public partial class RestrictionBuildingAssoc
    {
        public int IdBuilding { get; set; }
        public int IdRestriction { get; set; }
        public byte Deleted { get; set; }

        public virtual Building BuildingNavigation { get; set; }
        public virtual Restriction RestrictionNavigation { get; set; }
    }
}
