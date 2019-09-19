using System;

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

        public string GetTable() => "owner_sub_premises_assoc";
        public string GetFieldAdress() => "id_sub_premise";
        public int GetValueAddress() => IdSubPremise;

        public string GetAddress()
        {
            if (IdSubPremisesNavigation == null)
                throw new Exception("IdSubPremisesNavigation не подгружен");
            if (IdSubPremisesNavigation.IdPremisesNavigation == null)
                throw new Exception("IdPremisesNavigation не подгружен");
            if (IdSubPremisesNavigation.IdPremisesNavigation.IdPremisesTypeNavigation == null)
                throw new Exception("IdPremisesTypeNavigation не подгружен");
            if (IdSubPremisesNavigation.IdPremisesNavigation.IdBuildingNavigation == null)
                throw new Exception("IdBuildingNavigation не подгружен");
            if (IdSubPremisesNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation == null)
                throw new Exception("IdStreetNavigation не подгружен");
            var address =
                IdSubPremisesNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName +
                ", д." + IdSubPremisesNavigation.IdPremisesNavigation.IdBuildingNavigation.House + ", " +
                IdSubPremisesNavigation.IdPremisesNavigation.IdPremisesTypeNavigation.PremisesTypeShort +
                IdSubPremisesNavigation.IdPremisesNavigation.PremisesNum + ", к." +
                IdSubPremisesNavigation.SubPremisesNum;
            return address;
        }

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
