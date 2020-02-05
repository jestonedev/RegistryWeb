using RegistryWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class ReestrEmergencyPremisesFilter : FilterAddressOptions
    {
        public ReestrEmergencyPremisesFilter() : base()
        {
        }

        public ProcessOwnershipTypeEnum ProcessOwnershipType { get; set; }
        public string Persons { get; set; }

        public bool IsEmpty()
        {
            return IsAddressEmpty() &&
                ProcessOwnershipType == ProcessOwnershipTypeEnum.All &&
                string.IsNullOrWhiteSpace(Persons);
        }
        
    }
}
