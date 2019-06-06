using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class HeatingType
    {
        public HeatingType()
        {
            Buildings = new List<Buildings>();
        }

        public int IdHeatingType { get; set; }
        public string HeatingType1 { get; set; }
        public byte? Deleted { get; set; }

        public virtual IList<Buildings> Buildings { get; set; }
    }
}
