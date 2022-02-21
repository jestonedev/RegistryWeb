using RegistryDb.Models.Entities;
using System.Collections.Generic;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.ViewModel
{
    public class ChangeLogsVM : ListVM<ChangeLogsFilter>
    {
        public IEnumerable<ChangeLog> ChangeLogs { get; set; }
    }
}
