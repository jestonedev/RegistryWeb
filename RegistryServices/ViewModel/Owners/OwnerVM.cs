using RegistryDb.Models.Entities;
using RegistryWeb.Enums;

namespace RegistryServices.ViewModel.Owners
{
    public class OwnerVM
    {
        public Owner Owner { get; set; }
        public int I { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}