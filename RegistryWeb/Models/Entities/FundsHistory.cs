using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class FundsHistory
    {
        public FundsHistory()
        {
            FundsBuildingsAssoc = new HashSet<FundsBuildingsAssoc>();
            FundsPremisesAssoc = new HashSet<FundsPremisesAssoc>();
            FundsSubPremisesAssoc = new HashSet<FundsSubPremisesAssoc>();
        }

        public int IdFund { get; set; }
        public int IdFundType { get; set; }
        public string ProtocolNumber { get; set; }
        public DateTime? ProtocolDate { get; set; }
        public string IncludeRestrictionNumber { get; set; }
        public DateTime? IncludeRestrictionDate { get; set; }
        public string IncludeRestrictionDescription { get; set; }
        public string ExcludeRestrictionNumber { get; set; }
        public DateTime? ExcludeRestrictionDate { get; set; }
        public string ExcludeRestrictionDescription { get; set; }
        public string Description { get; set; }
        public byte? Deleted { get; set; }

        public virtual FundTypes IdFundTypeNavigation { get; set; }
        public virtual ICollection<FundsBuildingsAssoc> FundsBuildingsAssoc { get; set; }
        public virtual ICollection<FundsPremisesAssoc> FundsPremisesAssoc { get; set; }
        public virtual ICollection<FundsSubPremisesAssoc> FundsSubPremisesAssoc { get; set; }
    }
}
