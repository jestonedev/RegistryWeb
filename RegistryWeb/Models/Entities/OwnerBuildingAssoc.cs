using System;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerBuildingAssoc : IAddressAssoc, IEquatable<OwnerBuildingAssoc>
    {
        public int IdAssoc { get; set; }
        public int IdBuilding { get; set; }
        public int IdProcess { get; set; }
        public byte Deleted { get; set; }

        public virtual Building IdBuildingNavigation { get; set; }
        public virtual OwnerProcess IdProcessNavigation { get; set; }

        public bool Equals(OwnerBuildingAssoc oba)
        {
            if (oba == null)
                return false;
            if (ReferenceEquals(this, oba))
                return true;
            return IdAssoc == oba.IdAssoc && IdBuilding == oba.IdBuilding &&
                IdProcess == oba.IdProcess && Deleted == oba.Deleted;
        }
    }
}
