using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class FundTypes
    {
        public FundTypes()
        {
            FundsHistory = new HashSet<FundsHistory>();
        }

        public int IdFundType { get; set; }
        public string FundType { get; set; }

        public virtual ICollection<FundsHistory> FundsHistory { get; set; }
    }
}
