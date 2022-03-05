using RegistryDb.Interfaces;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;

namespace RegistryDb.Models.Entities.Tenancies
{
    public class TenancyBuildingAssoc : IBuildingAssoc
    {
        public int IdAssoc { get; set; }
        public int IdBuilding { get; set; }
        public int IdProcess { get; set; }
        public double? RentTotalArea { get; set; }
        public double? RentLivingArea { get; set; }
        public byte Deleted { get; set; }

        public virtual Building BuildingNavigation { get; set; }
        public virtual TenancyProcess ProcessNavigation { get; set; }
    }
}
