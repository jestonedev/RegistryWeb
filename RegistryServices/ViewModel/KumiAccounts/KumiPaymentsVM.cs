﻿using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryServices.Models.KumiPayments;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public class KumiPaymentsVM : ListVM<KumiPaymentsFilter>
    {
        public IEnumerable<KumiPayment> Payments { get; set; }
        public SelectList PaymentSourcesList { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Account { get; set; }
        public List<KumiPaymentDistributionInfoToObject> DistributionInfoToObjects { get; set; }
    }
}
