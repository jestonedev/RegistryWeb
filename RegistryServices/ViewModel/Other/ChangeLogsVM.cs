using RegistryDb.Models.Entities;
using System.Collections.Generic;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.ViewModel;

namespace RegistryServices.ViewModel.Other
{
    public class ChangeLogsVM : ListVM<ChangeLogsFilter>
    {
        public IEnumerable<ChangeLog> ChangeLogs { get; set; }
    }
}
