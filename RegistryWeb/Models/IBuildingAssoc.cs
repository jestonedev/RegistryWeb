using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models
{
    public interface IBuildingAssoc
    {
        int IdAssoc { get; set; }
        int IdBuilding { get; set; }
        int IdProcess { get; set; }
        Building BuildingNavigation { get; set; }
    }
}
