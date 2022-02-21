using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryDb.Models.Entities
{
    public class Judge
    {
        public Judge()
        {
            ClaimCourtOrders = new List<ClaimCourtOrder>();
            JudgeBuildingsAssoc = new List<JudgeBuildingAssoc>();
        }

        public int IdJudge { get; set; }
        public int NumDistrict { get; set; }
        public string Snp { get; set; }
        public string AddrDistrict { get; set; }
        public string PhoneDistrict { get; set; }
        public bool IsInactive { get; set; }
        public byte Deleted { get; set; }
        public virtual IList<ClaimCourtOrder> ClaimCourtOrders { get; set; }
        public virtual IList<JudgeBuildingAssoc> JudgeBuildingsAssoc { get; set; }
    }
}
