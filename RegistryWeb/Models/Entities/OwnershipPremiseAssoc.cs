using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnershipPremiseAssoc
    {
        public int IdPremises { get; set; }
        public int IdOwnershipRight { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnershipRight IdOwnershipRightNavigation { get; set; }
        public virtual Premise IdPremisesNavigation { get; set; }
    }
}
