using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{ 
    public class PremiseVM
    {
        public Premises Premise { get; set; }
        public IEnumerable<OwnershipRights> OwnershipRights { get; set; }
    }
}
