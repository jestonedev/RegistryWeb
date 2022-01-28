using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class KumiAccountState
    {
        public KumiAccountState()
        {
            KumiAccounts = new List<KumiAccount>();
        }

        public int IdState { get; set; }
        public string State { get; set; }

        public virtual IList<KumiAccount> KumiAccounts { get; set; }
    }
}
