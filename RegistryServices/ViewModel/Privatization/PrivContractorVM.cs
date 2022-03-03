using RegistryDb.Models.Entities;
using RegistryWeb.Enums;

namespace RegistryServices.ViewModel.Privatization
{
    public class PrivContractorVM
    {
        public PrivContractor PrivContractor { get; set; }
        public int I { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}