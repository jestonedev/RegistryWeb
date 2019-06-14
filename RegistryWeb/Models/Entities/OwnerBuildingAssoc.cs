using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerBuildingAssoc : IAddressAssoc
    {
        public int IdAssoc { get; set; }
        public int IdBuilding { get; set; }
        public int IdProcess { get; set; }
        public byte Deleted { get; set; }

        public virtual Building IdBuildingNavigation { get; set; }
        public virtual OwnerProcess IdProcessNavigation { get; set; }
    }
}
