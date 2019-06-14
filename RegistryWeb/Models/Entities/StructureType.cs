using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
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
