using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class StructureTypes
    {
        public StructureTypes()
        {
            Buildings = new HashSet<Buildings>();
        }

        public int IdStructureType { get; set; }
        public string StructureType { get; set; }
        public byte Deleted { get; set; }

        public virtual ICollection<Buildings> Buildings { get; set; }
    }
}
