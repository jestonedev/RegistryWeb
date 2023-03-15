using System.Collections.Generic;

namespace RegistryDb.Models.Entities.RegistryObjects.Buildings
{
    public class BuildingManagmentOrg
    {
        public BuildingManagmentOrg()
        {
            Buildings = new List<Building>();
        }

        public int IdOrganization { get; set; }
        public string Name { get; set; }

        public virtual IList<Building> Buildings { get; set; }
    }
}
