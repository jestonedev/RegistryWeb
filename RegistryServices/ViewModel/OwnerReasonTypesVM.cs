using System.Collections.Generic;
using RegistryDb.Models.Entities;
using RegistryWeb.ViewOptions;

namespace RegistryWeb.ViewModel
{
    public class OwnerReasonTypesVM<T>
    {
        public List<OwnerReasonType> ownerReasonTypes { get; set; }
        public OwnerReasonType ownerReasonType { get; set; }
        public PageOptions PageOptions { get; set; }
    }
}
