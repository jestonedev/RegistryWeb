using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities
{
    public partial class OwnershipRight
    {
        public OwnershipRight()
        {
            OwnershipBuildingsAssoc = new List<OwnershipBuildingAssoc>();
            OwnershipPremisesAssoc = new List<OwnershipPremiseAssoc>();
        }

        public int IdOwnershipRight { get; set; }
        [Required]
        public int IdOwnershipRightType { get; set; }
        public string Number { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public DateTime? ResettlePlanDate { get; set; }
        public DateTime? DemolishPlanDate { get; set; }
        public string FileOriginName { get; set; }
        public string FileDisplayName { get; set; }
        public string FileMimeType { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnershipRightType OwnershipRightTypeNavigation { get; set; }
        public virtual IList<OwnershipBuildingAssoc> OwnershipBuildingsAssoc { get; set; }
        public virtual IList<OwnershipPremiseAssoc> OwnershipPremisesAssoc { get; set; }
    }
}
