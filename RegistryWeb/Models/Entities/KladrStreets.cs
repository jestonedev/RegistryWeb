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
            Buildings = new List<Buildings>();
        }

        public string IdStreet { get; set; }
        public string StreetName { get; set; }
        public string StreetLong { get; set; }

        public virtual IList<Buildings> Buildings { get; set; }
    }
}
