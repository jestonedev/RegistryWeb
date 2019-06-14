using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public partial class KladrStreet
    {
        public KladrStreet()
        {
            Buildings = new List<Building>();
        }

        public string IdStreet { get; set; }
        public string StreetName { get; set; }
        public string StreetLong { get; set; }

        public virtual IList<Building> Buildings { get; set; }
    }
}
