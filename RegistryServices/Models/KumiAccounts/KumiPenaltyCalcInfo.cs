using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryServices.Models.KumiAccounts
{
    public class KumiPenaltyCalcInfo
    {
        public DateTime StartDate {get;set;}
        public DateTime EndDate { get; set; }
        public decimal KeyRate { get; set; }
        public decimal KeyRateCoef { get; set; }
        public decimal Sum { get; set; }
    }
}
