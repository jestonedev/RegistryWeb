using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class FundsBuildingsAssoc
    {
        public int IdBuilding { get; set; }
        public int IdFund { get; set; }
        public byte Deleted { get; set; }

        public virtual Buildings IdBuildingNavigation { get; set; }
        public virtual FundsHistory IdFundNavigation { get; set; }
    }
}
