using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public partial class HeatingType
    {
        public HeatingType()
        {
            Buildings = new List<Building>();
        }

        public int IdHeatingType { get; set; }
        public string HeatingType1 { get; set; }
        public byte? Deleted { get; set; }

        public virtual IList<Building> Buildings { get; set; }
    }
}
