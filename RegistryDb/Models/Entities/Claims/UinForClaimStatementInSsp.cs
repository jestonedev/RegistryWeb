using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryDb.Models.Entities.Claims
{
    public class UinForClaimStatementInSsp
    {
        public int Id { get; set; }
        public int IdClaim { get; set; }
        public string Uin { get; set; }
    }
}
