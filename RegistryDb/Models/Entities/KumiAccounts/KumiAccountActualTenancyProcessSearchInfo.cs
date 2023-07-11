using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    public class KumiAccountActualTenancyProcessSearchInfo
    {
        public int IdRecord { get; set; }
        public int IdAccount { get; set; }
        public int IdProcess { get; set; }
        public string Tenant { get; set; }
        public int Prescribed { get; set; }
        public string Emails { get; set; }
    }
}
