using RegistryWeb.Models.Entities;
using RegistryWeb.Models;
namespace RegistryWeb.ViewModel
{
    public class OwnershipRightVM : OwnershipRight
    {
        public Address Address { get; set; }

        public OwnershipRightVM() { }

        public OwnershipRightVM(OwnershipRight owr, Address address)
        {
            IdOwnershipRight = owr.IdOwnershipRight;
            IdOwnershipRightType = owr.IdOwnershipRightType;
            Number = owr.Number;
            Date = owr.Date;
            Description = owr.Description;
            ResettlePlanDate = owr.ResettlePlanDate;
            DemolishPlanDate = owr.DemolishPlanDate;
            Deleted = owr.Deleted;
            OwnershipRightTypeNavigation = owr.OwnershipRightTypeNavigation;
            OwnershipBuildingsAssoc = owr.OwnershipBuildingsAssoc;
            OwnershipPremisesAssoc = owr.OwnershipPremisesAssoc;
            Address = address;
        }
    }
}
