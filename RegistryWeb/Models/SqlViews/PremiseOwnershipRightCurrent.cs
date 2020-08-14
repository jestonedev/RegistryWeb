using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistryWeb.Models.SqlViews
{
    public class PremiseOwnershipRightCurrent
    {
        [Column("id_premises")]
        public int IdPremises { get; set; }
        public string ownership_right_type { get; set; }
        public int id_ownership_right { get; set; }
        [Column("id_ownership_right_type")]
        public int IdOwnershipRightType { get; set; }
        public string number { get; set; }
        public DateTime date { get; set; }
        public string description { get; set; }
        public DateTime? resettle_plan_date { get; set; }
        public DateTime? demolish_plan_date { get; set; }
    }
}