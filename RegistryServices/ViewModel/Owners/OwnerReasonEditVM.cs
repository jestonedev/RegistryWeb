using System.Collections.Generic;
using RegistryDb.Models.Entities;

namespace RegistryServices.ViewModel.Owners
{
    public class OwnerReasonEditVM
    {
        public OwnerReason OwnerReason { get;  set; }
        public IEnumerable<OwnerReasonType> OwnerReasonTypes { get; set; }
    }
}
