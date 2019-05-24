using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnershipPremisesAssoc
    {
        public int IdPremises { get; set; }
        public int IdOwnershipRight { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnershipRights IdOwnershipRightNavigation { get; set; }
        public virtual Premises IdPremisesNavigation { get; set; }
    }
}
