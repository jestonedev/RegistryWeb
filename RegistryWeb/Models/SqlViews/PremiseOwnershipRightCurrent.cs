using System;

namespace RegistryWeb.Models.SqlViews
{
    public class PremiseOwnershipRightCurrent
    {
        public int IdPremises { get; set; }
        public string OwnershipRightType { get; set; }
        public int IdOwnershipRight { get; set; }
        public int IdOwnershipRightType { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public DateTime? ResettlePlanDate { get; set; }
        public DateTime? DemolishPlanDate { get; set; }
    }
}