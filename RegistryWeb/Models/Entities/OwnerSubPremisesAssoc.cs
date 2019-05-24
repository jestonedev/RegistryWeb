using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerSubPremisesAssoc
    {
        public int IdAssoc { get; set; }
        public int IdSubPremises { get; set; }
        public int IdProcess { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerProcesses IdProcessNavigation { get; set; }
        public virtual SubPremises IdSubPremisesNavigation { get; set; }
    }
}
