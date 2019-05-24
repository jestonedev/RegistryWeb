using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public partial class KladrStreets
    {
        public KladrStreets()
        {
            Buildings = new HashSet<Buildings>();
        }

        public string IdStreet { get; set; }
        public string StreetName { get; set; }
        public string StreetLong { get; set; }

        public virtual ICollection<Buildings> Buildings { get; set; }
    }
}
