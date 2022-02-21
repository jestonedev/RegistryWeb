using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities
{
    public partial class FundHistory
    {
        public FundHistory()
        {
            FundsBuildingsAssoc = new List<FundBuildingAssoc>();
            FundsPremisesAssoc = new List<FundPremiseAssoc>();
            FundsSubPremisesAssoc = new List<FundSubPremiseAssoc>();
        }

        public int IdFund { get; set; }
        [Required(ErrorMessage = "Выберете тип найма")]
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

        public virtual FundType IdFundTypeNavigation { get; set; }
        public virtual IList<FundBuildingAssoc> FundsBuildingsAssoc { get; set; }
        public virtual IList<FundPremiseAssoc> FundsPremisesAssoc { get; set; }
        public virtual IList<FundSubPremiseAssoc> FundsSubPremisesAssoc { get; set; }
    }
}
