﻿using RegistryDb.Models.Entities.Claims;
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
        public decimal Value { get; set; }
        public virtual KumiPayment Payment { get; set; }
        public virtual Claim Claim { get; set; }
    }
}
