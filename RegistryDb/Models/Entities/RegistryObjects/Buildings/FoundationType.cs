using System.Collections.Generic;

namespace RegistryDb.Models.Entities.RegistryObjects.Buildings
{
    public class FoundationType
    {
        public FoundationType()
        {
            Buildings = new List<Building>();
        }

        public int IdFoundationType { get; set; }
        public string Name { get; set; }

        public virtual IList<Building> Buildings { get; set; }
    }
}
