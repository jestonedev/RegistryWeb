using System;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerPremiseAssoc : IAddressAssoc, IEquatable<OwnerPremiseAssoc>
    {
        public int IdAssoc { get; set; }
        public int IdPremise { get; set; }
        public int IdProcess { get; set; }
        public byte Deleted { get; set; }

        public virtual Premise IdPremisesNavigation { get; set; }
        public virtual OwnerProcess IdProcessNavigation { get; set; }

        public bool Equals(OwnerPremiseAssoc opa)
        {
            if (opa == null)
                return false;
            if (ReferenceEquals(this, opa))
                return true;
            return IdAssoc == opa.IdAssoc && IdPremise == opa.IdPremise &&
                IdProcess == opa.IdProcess && Deleted == opa.Deleted;
        }
    }
}
