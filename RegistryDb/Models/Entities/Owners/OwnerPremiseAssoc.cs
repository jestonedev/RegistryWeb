using RegistryDb.Interfaces;
using RegistryDb.Models.Entities.RegistryObjects.Premises;
using System;

namespace RegistryDb.Models.Entities.Owners
{
    public partial class OwnerPremiseAssoc : IPremiseAssoc, IAddressAssoc, IEquatable<OwnerPremiseAssoc>
    {
        public int IdAssoc { get; set; }
        public int IdPremise { get; set; }
        public int IdProcess { get; set; }
        public byte Deleted { get; set; }

        public virtual Premise PremiseNavigation { get; set; }
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
