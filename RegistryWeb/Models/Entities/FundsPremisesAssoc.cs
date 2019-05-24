﻿using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class FundsPremisesAssoc
    {
        public int IdPremises { get; set; }
        public int IdFund { get; set; }
        public byte Deleted { get; set; }

        public virtual FundsHistory IdFundNavigation { get; set; }
        public virtual Premises IdPremisesNavigation { get; set; }
    }
}
