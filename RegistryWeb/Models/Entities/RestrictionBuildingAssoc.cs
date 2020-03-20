using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class RestrictionBuildingAssoc
    {
        public int IdBuilding { get; set; }
        public int IdRestriction { get; set; }
        public byte Deleted { get; set; }

        public virtual Building BuildingNavigation { get; set; }
        public virtual Restriction RestrictionNavigation { get; set; }
    }
}
