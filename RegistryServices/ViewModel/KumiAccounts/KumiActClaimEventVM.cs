using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiActClaimEventVM : IChargePaymentEventVM
    {
        private DateTime _date;
        public DateTime Date { get => _date; set => _date = value; }
        public DateTime StartDate { get => _date; set => _date = value; }
        public decimal Tenancy { get; set; }
        public decimal TenancyTail { get; set; }
        public decimal Penalty { get; set; }
        public decimal PenaltyTail { get; set; }
        public int IdClaim { get; set; }
        public DateTime? StartDeptPeriod { get; set; }
        public DateTime? EndDeptPeriod { get; set; }
    }
}
