using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.ViewModel
{
    public class OwnerReasonEditVM
    {
        public OwnerReason OwnerReason { get;  set; }
        public IEnumerable<OwnerReasonType> OwnerReasonTypes { get; set; }
    }
}
