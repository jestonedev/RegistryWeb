using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerPremisesAssoc : IAddressAssoc
    {
        public int IdAssoc { get; set; }
        public int IdPremises { get; set; }
        public int IdProcess { get; set; }
        public byte Deleted { get; set; }

        public virtual Premises IdPremisesNavigation { get; set; }
        public virtual OwnerProcesses IdProcessNavigation { get; set; }
    }
}
