using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnershipRight
    {
        public OwnershipRight()
        {
            OwnershipBuildingsAssoc = new List<OwnershipBuildingAssoc>();
            OwnershipPremisesAssoc = new List<OwnershipPremiseAssoc>();
        }

        public int IdOwnershipRight { get; set; }
        public int IdOwnershipRightType { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public DateTime? ResettlePlanDate { get; set; }
        public DateTime? DemolishPlanDate { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnershipRightType IdOwnershipRightTypeNavigation { get; set; }
        public virtual IList<OwnershipBuildingAssoc> OwnershipBuildingsAssoc { get; set; }
        public virtual IList<OwnershipPremiseAssoc> OwnershipPremisesAssoc { get; set; }
    }
}
