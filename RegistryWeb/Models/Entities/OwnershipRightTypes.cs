using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnershipRightTypes
    {
        public OwnershipRightTypes()
        {
            OwnershipRights = new List<OwnershipRights>();
        }

        public int IdOwnershipRightType { get; set; }
        public string OwnershipRightType { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<OwnershipRights> OwnershipRights { get; set; }
    }
}
