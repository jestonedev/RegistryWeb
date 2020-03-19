using RegistryWeb.Models.Entities;
using RegistryWeb.Models;
namespace RegistryWeb.ViewModel
{
    public class OwnershipRightVM : OwnershipRight
    {
        public AddressTypes AddressType { get; set; }

        public OwnershipRightVM() { }

        public OwnershipRightVM(OwnershipRight owr, AddressTypes type)
        {
            IdOwnershipRight = owr.IdOwnershipRight;
            IdOwnershipRightType = owr.IdOwnershipRight;
            Number = owr.Number;
            Date = owr.Date;
            Description = owr.Description;
            ResettlePlanDate = owr.ResettlePlanDate;
            DemolishPlanDate = owr.DemolishPlanDate;
            Deleted = owr.Deleted;
            OwnershipRightTypeNavigation = owr.OwnershipRightTypeNavigation;
            OwnershipBuildingsAssoc = owr.OwnershipBuildingsAssoc;
            OwnershipPremisesAssoc = owr.OwnershipPremisesAssoc;
            AddressType = type;
        }
    }
}
