using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
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
