using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    public class EmergencyInfo
    {
        public DateTime? EmergencyDate { get; set; }
        public DateTime? ExcludeEmergencyDate { get; set; }
        public DateTime? DemolishedDate { get; set; }
    }
}
