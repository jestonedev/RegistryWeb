using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class RentTypeCategories
    {
        public int IdRentTypeCategory { get; set; }
        public int IdRentType { get; set; }
        public string RentTypeCategory { get; set; }

        public virtual RentTypes IdRentTypeNavigation { get; set; }
    }
}
