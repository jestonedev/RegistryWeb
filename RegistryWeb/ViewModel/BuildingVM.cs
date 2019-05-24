using RegistryWeb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{ 
    public class BuildingVM
    {
        public Buildings Building { get; set; }
        public IEnumerable<OwnershipRights> OwnershipRights { get; set; }
    }
}
