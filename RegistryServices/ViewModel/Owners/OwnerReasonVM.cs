using RegistryDb.Models.Entities.Owners;
using RegistryWeb.Enums;

namespace RegistryServices.ViewModel.Owners
{
    public class OwnerReasonVM
    {
        public OwnerReason OwnerReason { get; set; }
        public int I { get; set; }
        public int J { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}