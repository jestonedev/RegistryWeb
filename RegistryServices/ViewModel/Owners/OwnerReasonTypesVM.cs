using System.Collections.Generic;
using RegistryDb.Models.Entities.Owners;
using RegistryWeb.ViewOptions;

namespace RegistryServices.ViewModel.Owners
{
    public class OwnerReasonTypesVM<T>
    {
        public List<OwnerReasonType> ownerReasonTypes { get; set; }
        public OwnerReasonType ownerReasonType { get; set; }
        public PageOptions PageOptions { get; set; }
    }
}
