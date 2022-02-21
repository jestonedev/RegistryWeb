using RegistryDb.Models.Entities;
using RegistryWeb.Enums;

namespace RegistryWeb.ViewModel
{
    public class OwnerReasonVM
    {
        public OwnerReason OwnerReason { get; set; }
        public int I { get; set; }
        public int J { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}