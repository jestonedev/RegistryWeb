using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnershipRights
    {
        public OwnershipRights()
        {
            OwnershipBuildingsAssoc = new HashSet<OwnershipBuildingsAssoc>();
            OwnershipPremisesAssoc = new HashSet<OwnershipPremisesAssoc>();
        }

        public int IdOwnershipRight { get; set; }
        public int IdOwnershipRightType { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public DateTime? ResettlePlanDate { get; set; }
        public DateTime? DemolishPlanDate { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnershipRightTypes IdOwnershipRightTypeNavigation { get; set; }
        public virtual ICollection<OwnershipBuildingsAssoc> OwnershipBuildingsAssoc { get; set; }
        public virtual ICollection<OwnershipPremisesAssoc> OwnershipPremisesAssoc { get; set; }
    }
}
