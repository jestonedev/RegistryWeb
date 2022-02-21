using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class ReestrEmergencyPremisesVM : ListVM<ReestrEmergencyPremisesFilter>
    {
        public IEnumerable<ProcessOwnership> Reestr { get; set; }
    }
}
