using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class RentTypeCategory
    {
        public int IdRentTypeCategory { get; set; }
        public int IdRentType { get; set; }
        public string RentTypeCategoryName { get; set; }

        public virtual RentType IdRentTypeNavigation { get; set; }
    }
}
