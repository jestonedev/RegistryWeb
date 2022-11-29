using System;

namespace RegistryDb.Models.Entities.Claims
{
    public class LogClaimStatementInSpp
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string ExecutorLogin { get; set; }
        public int IdClaim { get; set; }
    }
}
