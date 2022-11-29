using System.Collections.Generic;

namespace RegistryDb.Models.Entities.RegistryObjects.Common.Ownerships
{
    public partial class OwnershipRightType
    {
        public OwnershipRightType()
        {
            OwnershipRights = new List<OwnershipRight>();
        }

        public int IdOwnershipRightType { get; set; }
        public string OwnershipRightTypeName { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<OwnershipRight> OwnershipRights { get; set; }
    }
}
