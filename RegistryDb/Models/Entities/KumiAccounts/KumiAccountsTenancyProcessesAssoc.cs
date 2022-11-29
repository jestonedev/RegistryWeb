using RegistryDb.Interfaces;
using RegistryDb.Models.Entities.RegistryObjects.Premises;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    public class KumiAccountsTenancyProcessesAssoc
    {
        public int IdAssoc { get; set; }
        public int IdAccount { get; set; }
        public int IdProcess { get; set; }
        public decimal Fraction { get; set; }
        public byte Deleted { get; set; }

        public virtual KumiAccount AccountNavigation { get; set; }
        public virtual TenancyProcess ProcessNavigation { get; set; }
    }
}