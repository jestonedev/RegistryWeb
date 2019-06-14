using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerPremiseAssoc : IAddressAssoc
    {
        public int IdAssoc { get; set; }
        public int IdPremises { get; set; }
        public int IdProcess { get; set; }
        public byte Deleted { get; set; }

        public virtual Premise IdPremisesNavigation { get; set; }
        public virtual OwnerProcess IdProcessNavigation { get; set; }
    }
}
