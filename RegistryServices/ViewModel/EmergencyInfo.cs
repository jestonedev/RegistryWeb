using System;

namespace RegistryWeb.ViewModel
{
    public class EmergencyInfo
    {
        public DateTime? EmergencyDate { get; set; }
        public DateTime? ExcludeEmergencyDate { get; set; }
        public DateTime? DemolishedDate { get; set; }
    }
}
