using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RegistryWeb.Models.SqlViews;

namespace RegistryWeb.Models.Entities
{
    public class PaymentAccount
    {
        public PaymentAccount()
        {
            Payments = new List<Payment>();
            PaymentAccountPremisesAssoc = new List<PaymentAccountPremiseAssoc>();
            PaymentAccountSubPremisesAssoc = new List<PaymentAccountSubPremiseAssoc>();
            Claims = new List<Claim>();
        }

        public int IdAccount { get; set; }
        public string Account { get; set; }
        public string Crn { get; set; }
        public string RawAddress { get; set; }
        public virtual IList<Payment> Payments { get; set; }
        public virtual IList<PaymentAccountPremiseAssoc> PaymentAccountPremisesAssoc { get; set; }
        public virtual IList<PaymentAccountSubPremiseAssoc> PaymentAccountSubPremisesAssoc { get; set; }
        public virtual IList<Claim> Claims { get; set; }
    }
}
