using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RegistryDb.Models.SqlViews;

namespace RegistryDb.Models.Entities
{
    public class KumiPaymentClaim
    {
        public KumiPaymentClaim()
        {
        }

        public int IdAssoc { get; set; }
        public int IdPayment { get; set; }
        public int IdClaim { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public virtual KumiPayment Payment { get; set; }
        public virtual Claim Claim { get; set; }
    }
}
