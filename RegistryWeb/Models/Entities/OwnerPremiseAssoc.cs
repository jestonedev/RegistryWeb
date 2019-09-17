using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerPremiseAssoc : IAddressAssoc
    {
        public int IdAssoc { get; set; }
        public int IdPremise { get; set; }
        public int IdProcess { get; set; }
        public byte Deleted { get; set; }

        public virtual Premise IdPremisesNavigation { get; set; }
        public virtual OwnerProcess IdProcessNavigation { get; set; }

        public string GetTable() => "owner_premises_assoc";
        public string GetFieldAdress() => "id_premise";
        public int GetValueAddress() => IdPremise;

        public string GetAddress()
        {
            if (IdPremisesNavigation == null)
                throw new System.Exception("IdPremisesNavigation не подгружен");
            if (IdPremisesNavigation.IdPremisesTypeNavigation == null)
                throw new System.Exception("IdPremisesTypeNavigation не подгружен");
            if (IdPremisesNavigation.IdBuildingNavigation == null)
                throw new System.Exception("IdBuildingNavigation не подгружен");
            if (IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation == null)
                throw new System.Exception("IdStreetNavigation не подгружен");
            var address =
                IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName + ", д." +
                IdPremisesNavigation.IdBuildingNavigation.House + ", " +
                IdPremisesNavigation.IdPremisesTypeNavigation.PremisesTypeShort +
                IdPremisesNavigation.PremisesNum;
            return address;
        }
    }
}
