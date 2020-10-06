using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class OwnerReasonVM
    {
        public OwnerReason OwnerReason { get; set; }
        public int IOwner { get; set; }
        public int IReason { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}