using RegistryDb.Models.Entities;
using RegistryWeb.ViewModel;

namespace RegistryServices.ViewModel.RegistryObjects
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
            FileOriginName = owr.FileOriginName;
            FileDisplayName = owr.FileDisplayName;
            FileMimeType = owr.FileMimeType;
            Deleted = owr.Deleted;
            OwnershipRightTypeNavigation = owr.OwnershipRightTypeNavigation;
            OwnershipBuildingsAssoc = owr.OwnershipBuildingsAssoc;
            OwnershipPremisesAssoc = owr.OwnershipPremisesAssoc;
            Address = address;
        }
    }
}
