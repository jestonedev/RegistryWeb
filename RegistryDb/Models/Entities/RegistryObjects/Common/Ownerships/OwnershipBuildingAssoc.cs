using RegistryDb.Models.Entities.RegistryObjects.Buildings;

namespace RegistryDb.Models.Entities.RegistryObjects.Common.Ownerships
{
    public partial class OwnershipBuildingAssoc
    {
        public int IdBuilding { get; set; }
        public int IdOwnershipRight { get; set; }
        public byte Deleted { get; set; }

        public virtual Building BuildingNavigation { get; set; }
        public virtual OwnershipRight OwnershipRightNavigation { get; set; }
    }
}
