using RegistryWeb.Models.Entities;
using RegistryWeb.Models;
namespace RegistryWeb.ViewModel
{
    public class RestrictionVM : Restriction
    {
        public AddressTypes AddressType { get; set; }

        public RestrictionVM() { }

        public RestrictionVM(Restriction owr, AddressTypes type)
        {
            IdRestriction = owr.IdRestriction;
            IdRestrictionType = owr.IdRestrictionType;
            Number = owr.Number;
            Date = owr.Date;
            Description = owr.Description;
            DateStateReg = owr.DateStateReg;
            Deleted = owr.Deleted;
            RestrictionTypeNavigation = owr.RestrictionTypeNavigation;
            RestrictionBuildingsAssoc = owr.RestrictionBuildingsAssoc;
            RestrictionPremisesAssoc = owr.RestrictionPremisesAssoc;
            AddressType = type;
        }
    }
}
