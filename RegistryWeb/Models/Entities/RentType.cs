using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class RentType
    {
        public RentType()
        {
            RentTypeCategories = new List<RentTypeCategory>();
        }

        public int IdRentType { get; set; }
        public string RentTypeName { get; set; }
        public string RentTypeShort { get; set; }
        public string RentTypeGenetive { get; set; }

        public virtual IList<RentTypeCategory> RentTypeCategories { get; set; }
    }
}
