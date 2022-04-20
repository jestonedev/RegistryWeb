using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryWeb.Models.Entities
{
    public class UinForClaimStatementInSsp
    {
        public int Id { get; set; }
        public int IdClaim { get; set; }
        public string Uin { get; set; }
    }
}
