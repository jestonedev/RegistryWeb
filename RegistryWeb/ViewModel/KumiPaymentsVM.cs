using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryDb.Models.SqlViews;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace RegistryWeb.ViewModel
{
    public class KumiPaymentsVM : ListVM<KumiPaymentsFilter>
    {
        public IEnumerable<KumiPayment> Payments { get; set; }
    }
}
