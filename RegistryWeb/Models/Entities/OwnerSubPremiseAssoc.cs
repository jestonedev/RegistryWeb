using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerSubPremiseAssoc : IAddressAssoc
    {
        public int IdAssoc { get; set; }
        public int IdSubPremises { get; set; }
        public int IdProcess { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerProcess IdProcessNavigation { get; set; }
        public virtual SubPremise IdSubPremisesNavigation { get; set; }
    }
}
