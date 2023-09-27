﻿using RegistryDb.Models.Entities.Claims;
using System;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    [Serializable]
    public class KumiPaymentUntied
    {
        public KumiPaymentUntied()
        {
        }

        public int IdRecord { get; set; }
        public int IdPayment { get; set; }
        public int IdCharge { get; set; }
        public int? IdClaim { get; set; }
        public DateTime TiedDate { get; set; }
        public DateTime UntiedDate { get; set; }

        // Найм
        public decimal TenancyValue { get; set; }
        public decimal PenaltyValue { get; set; }
        
        // ДГИ
        public decimal DgiValue { get; set; }

        // ПКК
        public decimal PkkValue { get; set; }

        // Падун
        public decimal PadunValue { get; set; }

        public virtual KumiPayment Payment { get; set; }
        public virtual KumiCharge Charge { get; set; }
        public virtual Claim Claim { get; set; }
    }
}
