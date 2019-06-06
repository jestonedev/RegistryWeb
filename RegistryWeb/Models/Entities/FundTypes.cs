using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class FundTypes
    {
        public FundTypes()
        {
            FundsHistory = new List<FundsHistory>();
        }

        public int IdFundType { get; set; }
        public string FundType { get; set; }

        public virtual IList<FundsHistory> FundsHistory { get; set; }
    }
}
