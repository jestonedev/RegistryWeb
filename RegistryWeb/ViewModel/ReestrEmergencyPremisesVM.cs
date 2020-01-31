using RegistryWeb.Models;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class ReestrEmergencyPremisesVM : ListVM<ReestrEmergencyPremisesFilter>
    {
        public IEnumerable<ProcessOwnership> Reestr { get; set; }
    }
}
