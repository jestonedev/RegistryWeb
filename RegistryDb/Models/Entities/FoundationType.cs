using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryDb.Models.Entities
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
