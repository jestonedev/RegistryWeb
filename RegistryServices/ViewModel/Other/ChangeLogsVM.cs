using System.Collections.Generic;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.ViewModel;
using RegistryDb.Models.Entities.Common;

namespace RegistryServices.ViewModel.Other
{
    public class ChangeLogsVM : ListVM<ChangeLogsFilter>
    {
        public IEnumerable<ChangeLog> ChangeLogs { get; set; }
    }
}
