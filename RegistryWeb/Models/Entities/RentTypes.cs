using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class RentTypes
    {
        public RentTypes()
        {
            RentTypeCategories = new HashSet<RentTypeCategories>();
        }

        public int IdRentType { get; set; }
        public string RentType { get; set; }
        public string RentTypeShort { get; set; }
        public string RentTypeGenetive { get; set; }

        public virtual ICollection<RentTypeCategories> RentTypeCategories { get; set; }
    }
}
