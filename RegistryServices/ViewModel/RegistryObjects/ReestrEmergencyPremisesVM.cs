using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.RegistryObjects
{
    public class ReestrEmergencyPremisesVM : ListVM<ReestrEmergencyPremisesFilter>
    {
        public IEnumerable<ProcessOwnership> Reestr { get; set; }
    }
}
