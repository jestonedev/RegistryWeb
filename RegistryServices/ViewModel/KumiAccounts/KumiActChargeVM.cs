using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiActChargeVM
    {
        public DateTime Date { get; set; }  // Период (оно же - дата начисления - EndDate)
        public decimal Value { get; set; }  // Начисление в указанном периоде
        public List<IChargeEventVM> Events { get; set; }
    }
}
