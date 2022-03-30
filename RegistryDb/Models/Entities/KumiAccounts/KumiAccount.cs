using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.Entities.Tenancies;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    public class KumiAccount : AccountBase
    {
        public KumiAccount()
        {
            TenancyProcesses = new List<TenancyProcess>();
            Claims = new List<Claim>();
            Charges = new List<KumiCharge>();
            IdState = 1;
        }

        [Required(ErrorMessage = "Укажите состояние")]
        public int IdState { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? AnnualDate { get; set; }
        public byte RecalcMarker { get; set; }
        public string RecalcReason { get; set; }
        public DateTime? LastChargeDate { get; set; }
        public decimal? CurrentBalanceTenancy { get; set; }
        public decimal? CurrentBalancePenalty { get; set; }
        public byte Deleted { get; set; }
        public virtual KumiAccountState State { get; set; }
        public virtual KumiAccountAddress KumiAccountAddressNavigation { get; set; }
        public virtual IList<TenancyProcess> TenancyProcesses { get; set; }
        public virtual IList<Claim> Claims { get; set; }
        public virtual IList<KumiCharge> Charges { get; set; }
       
        [NotMapped]
        public Dictionary<int, DateTime> MonthsList { get; set; }
    }
}
