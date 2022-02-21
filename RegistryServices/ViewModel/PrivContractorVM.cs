using RegistryDb.Models.Entities;
using RegistryWeb.Enums;

namespace RegistryWeb.ViewModel
{
    public class PrivContractorVM
    {
        public PrivContractor PrivContractor { get; set; }
        public int I { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}