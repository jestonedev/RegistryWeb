using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public partial class OwnershipBuildingAssoc
    {
        public int IdBuilding { get; set; }
        public int IdOwnershipRight { get; set; }
        public byte Deleted { get; set; }

        public virtual Building BuildingNavigation { get; set; }
        public virtual OwnershipRight OwnershipRightNavigation { get; set; }
    }
}
