using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RegistryWeb.Models.SqlViews;

namespace RegistryWeb.Models.Entities
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
        public virtual IList<TenancyProcess> TenancyProcesses { get; set; }
        public virtual IList<Claim> Claims { get; set; }
        public virtual IList<KumiCharge> Charges { get; set; }
    }
}
