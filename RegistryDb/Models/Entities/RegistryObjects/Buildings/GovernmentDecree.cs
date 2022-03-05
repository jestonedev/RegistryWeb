using System.Collections.Generic;

namespace RegistryDb.Models.Entities.RegistryObjects.Buildings
{
    public partial class GovernmentDecree
    {
        public GovernmentDecree()
        {
            Buildings = new List<Building>();
        }

        public int IdDecree { get; set; }
        public string Number { get; set; }

        public virtual IList<Building> Buildings { get; set; }
    }
}
