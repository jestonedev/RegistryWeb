using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public partial class FundType
    {
        public FundType()
        {
            FundsHistory = new List<FundHistory>();
        }

        public int IdFundType { get; set; }
        public string FundTypeName { get; set; }

        public virtual IList<FundHistory> FundsHistory { get; set; }
    }
}
