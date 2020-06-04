using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.ViewModel
{
    public class TenancyProcessVM
    {
        public TenancyProcess TenancyProcess { get; set; }
        public IList<RentType> RentTypes { get; set; }
        public IList<RentTypeCategory> RentTypeCategories { get; set; }
        public IList<Kinship> Kinships { get; set; }
        public IList<TenancyReasonType> TenancyReasonTypes { get; set; }
        public IList<Executor> Executors { get; set; }
    }
}