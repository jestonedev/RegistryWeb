using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnershipBuildingsAssoc
    {
        public int IdBuilding { get; set; }
        public int IdOwnershipRight { get; set; }
        public byte Deleted { get; set; }

        public virtual Buildings IdBuildingNavigation { get; set; }
        public virtual OwnershipRights IdOwnershipRightNavigation { get; set; }
    }
}
