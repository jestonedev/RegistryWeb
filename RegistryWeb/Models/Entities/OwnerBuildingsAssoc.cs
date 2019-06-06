using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerBuildingsAssoc : IAddressAssoc
    {
        public int IdAssoc { get; set; }
        public int IdBuilding { get; set; }
        public int IdProcess { get; set; }
        public byte Deleted { get; set; }

        public virtual Buildings IdBuildingNavigation { get; set; }
        public virtual OwnerProcesses IdProcessNavigation { get; set; }
    }
}
