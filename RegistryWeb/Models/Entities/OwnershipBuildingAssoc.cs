using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnershipBuildingAssoc
    {
        public int IdBuilding { get; set; }
        public int IdOwnershipRight { get; set; }
        public byte Deleted { get; set; }

        public virtual Building IdBuildingNavigation { get; set; }
        public virtual OwnershipRight IdOwnershipRightNavigation { get; set; }
    }
}
