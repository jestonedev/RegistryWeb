using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    public class KumiAccountAddressInfix
    {
        public int IdRecord { get; set; }
        public int IdAccount { get; set; }
        public string Infix { get; set; }
        public string Address { get; set; }
        public double TotalArea { get; set; }
    }
}
