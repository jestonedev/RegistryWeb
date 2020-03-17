using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{ 
    public class PremiseVM
    {
        public Premise Premise { get; set; }
        public IEnumerable<OwnershipRight> OwnershipRights { get; set; }

    }
}
