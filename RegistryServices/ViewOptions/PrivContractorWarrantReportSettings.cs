using RegistryWeb.Enums;

namespace RegistryWeb.ViewOptions
{
    public class PrivContractorWarrantReportSettings
    {
        public int IdContractor { get; set; }
        public int? IdRealtor { get; set; }
        public PrivContractorWarrantTypeEnum WarrantType { get; set; }
    }
}
