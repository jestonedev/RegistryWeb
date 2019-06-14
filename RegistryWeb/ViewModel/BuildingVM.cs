using RegistryWeb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{ 
    public class BuildingVM
    {
        public Building Building { get; set; }
        public IEnumerable<OwnershipRight> OwnershipRights { get; set; }
    }
}
