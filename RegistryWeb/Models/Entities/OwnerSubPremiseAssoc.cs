﻿using System;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerSubPremiseAssoc : IAddressAssoc, IEquatable<OwnerSubPremiseAssoc>
    {
        public int IdAssoc { get; set; }
        public int IdSubPremise { get; set; }
        public int IdProcess { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerProcess IdProcessNavigation { get; set; }
        public virtual SubPremise IdSubPremisesNavigation { get; set; }

        public bool Equals(OwnerSubPremiseAssoc ospa)
        {
            if (ospa == null)
                return false;
            if (ReferenceEquals(this, ospa))
                return true;
            return IdAssoc == ospa.IdAssoc && IdSubPremise == ospa.IdSubPremise &&
                IdProcess == ospa.IdProcess && Deleted == ospa.Deleted;
        }
    }
}
