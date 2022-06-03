using RegistryDb.Models.Entities.Tenancies;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    public class KumiAccountAddress
    {
        public int IdAccount { get; set; }
        public string Address { get; set; }
        public int IdProcess { get; set; }
        public virtual KumiAccount KumiAccountNavigation { get; set; }
    }
}
