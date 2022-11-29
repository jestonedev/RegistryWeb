using RegistryDb.Models.Entities.Common;
using System;

namespace RegistryDb.Models.Entities.Claims
{
    public class ClaimCourtOrder
    {
        public int IdOrder { get; set; }
        public int IdClaim { get; set; }
        public int IdExecutor { get; set; }
        public DateTime? CreateDate { get; set; }
        public int IdSigner { get; set; }
        public int IdJudge { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime OpenAccountDate { get; set; }
        public decimal? AmountTenancy { get; set; }
        public decimal? AmountPenalties { get; set; }
        public decimal? AmountDgi { get; set; }
        public decimal? AmountPadun { get; set; }
        public decimal? AmountPkk { get; set; }
        public DateTime? StartDeptPeriod { get; set; }
        public DateTime? EndDeptPeriod { get; set; }
        public virtual Claim IdClaimNavigation { get; set; }
        public virtual Executor IdExecutorNavigation { get; set; }
        public virtual SelectableSigner IdSignerNavigation { get; set; }
        public virtual Judge IdJudgeNavigation { get; set; }
        public byte Deleted { get; set; }
    }
}
