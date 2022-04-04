using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    public class KumiKeyRate
    {
        public KumiKeyRate()
        {
        }

        public int IdRecord { get; set; }
        public DateTime StartDate { get; set; }
        public decimal Value { get; set; }
        
    }
}