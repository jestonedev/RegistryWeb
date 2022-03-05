using System.Collections.Generic;

namespace RegistryDb.Models.Entities.KumiAccounts
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
