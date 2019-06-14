using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class FundPremiseAssoc
    {
        public int IdPremises { get; set; }
        public int IdFund { get; set; }
        public byte Deleted { get; set; }

        public virtual FundHistory IdFundNavigation { get; set; }
        public virtual Premise IdPremisesNavigation { get; set; }
    }
}
