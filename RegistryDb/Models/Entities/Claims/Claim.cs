﻿using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.Payments;
using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.Claims
{
    public class Claim
    {
        public Claim()
        {
            ClaimStates = new List<ClaimState>();
            ClaimPersons = new List<ClaimPerson>();
            ClaimFiles = new List<ClaimFile>();
        }

        public int IdClaim { get; set; }
        public int? IdAccount { get; set; }
        public int? IdAccountAdditional { get; set; }
        public int? IdAccountKumi { get; set; }
        public decimal? AmountTenancy { get; set; }
        public decimal? AmountPenalties { get; set; }
        public decimal? AmountDgi { get; set; }
        public decimal? AmountPadun { get; set; }
        public decimal? AmountPkk { get; set; }
        public DateTime? AtDate { get; set; }
        public DateTime? StartDeptPeriod { get; set; }
        public DateTime? EndDeptPeriod { get; set; }
        public string Description { get; set; }
        public bool EndedForFilter { get; set; }
        public decimal? LastAccountBalanceOutput { get; set; }
        public byte Deleted { get; set; }
        public virtual PaymentAccount IdAccountNavigation { get; set; }
        public virtual PaymentAccount IdAccountAdditionalNavigation { get; set; }
        public virtual KumiAccount IdAccountKumiNavigation { get; set; }
        public virtual ICollection<ClaimState> ClaimStates { get; set; }
        public virtual ICollection<ClaimPerson> ClaimPersons { get; set; }
        public virtual IList<ClaimFile> ClaimFiles { get; set; }
        public virtual IList<ClaimCourtOrder> ClaimCourtOrders { get; set; }
        public virtual IList<KumiPaymentClaim> PaymentClaims { get; set; }
    }
}
