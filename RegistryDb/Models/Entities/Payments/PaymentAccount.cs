using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.Common;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.Payments
{
    public class PaymentAccount: AccountBase
    {
        public PaymentAccount()
        {
            Payments = new List<Payment>();
            PaymentAccountPremisesAssoc = new List<PaymentAccountPremiseAssoc>();
            PaymentAccountSubPremisesAssoc = new List<PaymentAccountSubPremiseAssoc>();
            Claims = new List<Claim>();
        }
        public string Crn { get; set; }
        public string RawAddress { get; set; }
        public int? Prescribed { get; set; }
        public string Tenant { get; set; }
        public virtual IList<Payment> Payments { get; set; }
        public virtual IList<PaymentAccountPremiseAssoc> PaymentAccountPremisesAssoc { get; set; }
        public virtual IList<PaymentAccountSubPremiseAssoc> PaymentAccountSubPremisesAssoc { get; set; }
        public virtual IList<Claim> Claims { get; set; }
        public virtual IList<Claim> ClaimsByAdditional { get; set; }
    }
}
