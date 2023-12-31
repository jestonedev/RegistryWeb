﻿using System;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiActPeniCalcEventVM : IChargeEventVM
    {
        private DateTime _date;

        public DateTime Date { get => _date; set => _date = value; }
        public DateTime StartDate { get => _date; set => _date = value; }
        public DateTime EndDate { get; set; }
        public decimal KeyRate { get; set; }
        public decimal KeyRateCoef { get; set; }
        public decimal Tenancy { get; set; }
        public decimal Penalty { get; set; }
    }
}
