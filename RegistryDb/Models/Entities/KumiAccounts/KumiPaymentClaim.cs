using RegistryDb.Models.Entities.Claims;
using System;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    [Serializable]
    public class KumiPaymentClaim
    {
        public KumiPaymentClaim()
        {
        }

        public int IdAssoc { get; set; }
        public int IdPayment { get; set; }
        public int IdClaim { get; set; }
        public DateTime Date { get; set; }
        public decimal TenancyValue { get; set; }
        public decimal PenaltyValue { get; set; }
        public virtual KumiPayment Payment { get; set; }
        public virtual Claim Claim { get; set; }
        public int? IdDisplayCharge { get; set; }
        public virtual KumiCharge DisplayCharge { get; set; }
    }
}
