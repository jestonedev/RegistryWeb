using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;

namespace RegistryDb.Interfaces
{
    public interface IBuildingAssoc
    {
        int IdAssoc { get; set; }
        int IdBuilding { get; set; }
        int IdProcess { get; set; }
        Building BuildingNavigation { get; set; }
    }
}
