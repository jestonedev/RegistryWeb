using RegistryWeb.Enums;

namespace RegistryWeb.ViewOptions
{
    public class PrivQuarterReportSettings
    {
        public int Year { get; set; }
        public int? Quarter { get; set; }
        public int? Month { get; set; }
        public PrivQuarterReportFilterFieldEnum FilterField { get; set; }
    }
}
