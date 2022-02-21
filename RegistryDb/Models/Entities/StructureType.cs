using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public partial class StructureType
    {
        public StructureType()
        {
            Buildings = new List<Building>();
        }

        public int IdStructureType { get; set; }
        public string StructureTypeName { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<Building> Buildings { get; set; }
    }
}
